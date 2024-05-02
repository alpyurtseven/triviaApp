using System.Data;
using Microsoft.AspNetCore.SignalR;
using triviaApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using Newtonsoft.Json;
using triviaApp.Utils;
using System.Linq.Expressions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace triviaApp.Hubs
{
	public class GameHub: Hub<IGameHub>
    {
        private static ConcurrentQueue<string> participantQueue = new ConcurrentQueue<string>();
        private static ConcurrentDictionary<string, string> clientList = new ConcurrentDictionary<string, string>();
        private static Dictionary<int, Queue<Question>> categoryQuestions = new Dictionary<int, Queue<Question>>();
        private static ConcurrentDictionary<string, int> activeQuestions = new ConcurrentDictionary<string, int>();
        private static ConcurrentBag<int> removedCategoryIds = new ConcurrentBag<int>();
        private static ConcurrentDictionary<string, string> adminList = new ConcurrentDictionary<string, string>();
        private static int _competitionId;
        private static int _questionId;
        private static bool _gameStarted;
        private static string _lastScreenName = null;
        private static Question _lastSelectedQuestion;
        private static List<string> _competitionParticipants;
        private static Competition _competition;


        private readonly IServiceScopeFactory _serviceProvider;
        private readonly AppDbContext _context;

        public GameHub(AppDbContext context, IServiceScopeFactory serviceProvider)
		{
            _context = context;
            _serviceProvider = serviceProvider;

        }

        public async Task SetUsername(string username, int competitionId)
        {
            var connectionId = Context.ConnectionId;

            var existingParticipant = await _context.Participants
                .FirstOrDefaultAsync(p => p.Username == username && p.CompetitionId == competitionId);

            if (existingParticipant != null)
            {
                await Clients.Caller.Error("Bu kullanıcı adı bu yarışmada zaten kayıtlı.");
                return;
            }

            clientList.TryAdd(username, connectionId);
            participantQueue.Enqueue(username);

            var participant = new Participant
            {
                Username = username,
                CompetitionId = competitionId
            };
            _context.Participants.Add(participant);

            await _context.SaveChangesAsync();

            var score = new Score
            {
                ParticipantId = participant.Id,
                Points = 0,
                CompetitionId = competitionId
            };
            _context.Scores.Add(score);

            await _context.SaveChangesAsync();

            await Groups.AddToGroupAsync(connectionId, "Participants");

            await Clients.Caller.JoinGame(username, connectionId);

            await Clients.Caller.ReceiveUIUpdate("waitingroom", clientList.ToList(), true);
            await Clients.GroupExcept("Participants", connectionId).ReceiveUIUpdate("waitingroom", clientList.ToList(), false);
            await Clients.Group("Admins").ReceiveUIUpdate("adminwaitingroom", clientList.ToList(), false);
        }

        public async Task SelectCategory(int categoryId, int competitionId, string username)
        {
            if (!participantQueue.TryPeek(out string nextParticipant) || nextParticipant != username)
            {
                await Clients.Caller.Error("Sıra sizde değil!");
                return;
            }

            if (!categoryQuestions.ContainsKey(categoryId) && !removedCategoryIds.Contains(categoryId))
            {
                var competition = await _context.Competitions
                  .AsNoTracking()
                  .Include(z => z.CompetitionQuestions)
                  .ThenInclude(z => z.Question)
                  .ThenInclude(z => z.Category)
                  .Include(z => z.CompetitionCategories)
                  .ThenInclude(z => z.Category)
                  .Where(z => z.Id == competitionId).FirstOrDefaultAsync();
                var questions = competition.CompetitionQuestions
                   .Where(q => q.CompetitionId == competitionId && q.Question.CategoryId == categoryId)
                   .OrderBy(q => q.Question.Difficulty)
                   .Select(q => q.Question)
                   .ToList();

                var questionQueue = new Queue<Question>(questions);
                categoryQuestions.Add(categoryId, questionQueue);
            }

            if (categoryQuestions.Count() <= 0)
            {
                await Clients.Caller.Error("Yarışma bitti");
                return;
            }


            if (categoryQuestions[categoryId].TryDequeue(out Question question))
            {
                if (categoryQuestions[categoryId].Count == 0)
                {
                    categoryQuestions.Remove(categoryId);
                    removedCategoryIds.Add(categoryId);

                    await Clients.Group("Participants").SendCategoriesThatHasNoQuestion(categoryId);
                }

                _lastSelectedQuestion = question;

                _lastScreenName = "question";

                activeQuestions.Clear();

                await Clients.Group("Participants").ReceiveUIUpdate("question",SerializeObject(question), true);

                await StartTimer(question);

                participantQueue.TryDequeue(out string activeParticipant);
                participantQueue.Enqueue(activeParticipant);
            }
            else
            {
                await Clients.Caller.Error("Soru kalmadı!");
            }
        }

        public async Task AnswerQuestion(string username, int questionId, string answer, int competitionId)
        {
            _competitionId = competitionId;
            _questionId = questionId;

            var question = await _context.Questions.FindAsync(questionId);
            if (question == null)
            {
                await Clients.Caller.Error("Soru bulunamadı.");
                return;
            }

            var participant = await _context.Participants.FirstOrDefaultAsync(p => p.Username == username && p.CompetitionId == competitionId);
            if (participant == null)
            {
                await Clients.Caller.Error("Katılımcı bulunamadı.");
                return;
            }

            var score = await _context.Scores.FirstOrDefaultAsync(s => s.ParticipantId == participant.Id && s.CompetitionId == competitionId);
            if (score == null)
            {
                score = new Score { ParticipantId = participant.Id, CompetitionId = competitionId, Points = 0 };
                _context.Scores.Add(score);
            }

            if (question.RightAnswer == answer)
            {
                score.Points += question.Points; 
                activeQuestions.TryAdd(username, questionId);
            }
            else
            {
                activeQuestions.TryAdd(username, -1);
            }

            await _context.SaveChangesAsync();

            await Clients.Groups("Admins").UserAnswer(username, answer);

            await CheckAllAnswers(competitionId, questionId, false, false);
        }

        public async Task CheckAllAnswers(int competitionId, int questionId, bool timeOut, bool isReconnect)
        {
            var participantsInQueue = new HashSet<string>(participantQueue);
            int totalParticipants = 999;

            if (isReconnect)
            {
                var longRunningScope = _serviceProvider.CreateScope();
                Task.Run(async () => {
                    try
                    {
                        var context = longRunningScope.ServiceProvider.GetRequiredService<AppDbContext>();
                        totalParticipants = await context.Participants
                          .Where(p => p.CompetitionId == competitionId && participantsInQueue.Contains(p.Username))
                          .CountAsync();
                    }
                    finally
                    {
                        longRunningScope.Dispose();
                    }
                }).Wait();
            }
            else
            {
                 totalParticipants = await _context.Participants
                    .Where(p => p.CompetitionId == competitionId && participantsInQueue.Contains(p.Username))
                    .CountAsync();
            }


          
            if (activeQuestions.Count >= totalParticipants || timeOut || isReconnect)
            {
                CountdownTimer.Dispose();

                var correctAnswers = activeQuestions.Where(q => q.Value == questionId).Select(q => q.Key).ToList();
                var wrongAnswers = activeQuestions.Where(q => q.Value == -1).Select(q => q.Key).ToList();
                bool isCompetitionOver = await AreCompetitionQuestionsOver();

                if (!isReconnect)
                {
                   
                    _lastScreenName = "questionResults";

                    Thread.Sleep(2000);

                    await Clients.Group("Participants").QuestionResults(SerializeObject(correctAnswers), SerializeObject(wrongAnswers), isCompetitionOver);
                }
                else
                {
                    await Clients.Caller.QuestionResults(SerializeObject(correctAnswers), SerializeObject(wrongAnswers), isCompetitionOver);
                }
            }
        }

        public async Task UpdateParticipantsScores(int competitionId)
        {
            var participantsScores = _context.Scores
                .AsNoTracking()
                .Include(s => s.Participant)
                .Where(z => z.CompetitionId == competitionId).ToList()
                .Select(s => new { s.Participant.Username, s.Points });

            participantQueue.TryPeek(out string nextParticipant);

            var participantsInQueue = new HashSet<string>(participantQueue);

            var filteredScores = participantsScores
                .Where(s => participantsInQueue.Contains(s.Username))
                .ToList();

            await Clients.Group("Participants").ReceiveParticipantsScores(SerializeObject(filteredScores), nextParticipant);
        }

        private async Task NotifyNextParticipant()
        {
            if (participantQueue.TryPeek(out string nextParticipant))
            {
                string nextConnectionId = clientList[nextParticipant];
                await Clients.Client(nextConnectionId).SetYourTurn(true);
            }
        }

        public async Task StartTimer(Question question)
        {
            CountdownTimer.OnSecondPassed += (seconds) =>
            {
                Clients.Group("Participants").SetTime(seconds.ToString());
            };

            CountdownTimer.OnTimerCompleted += async () =>
            {
                await Clients.Group("Participants").EndTime();
                await Clients.Group("Admins").QuestionTimeEnded();
            };

            CountdownTimer.Start(question.Time);
        }

        public async Task GetLastScreen()
        {
            switch (_lastScreenName)
            {
                case "question":
                    Clients.Caller.ReceiveUIUpdate(_lastScreenName, SerializeObject(_lastSelectedQuestion), true);
                    break;
                case "questionResults":
                    Clients.Caller.ReceiveUIUpdate("question", SerializeObject(_lastSelectedQuestion), true);

                    CheckAllAnswers(_competitionId, _questionId, false, true);
                    break;
                case "selectcategory":
                    await Clients.Caller.ReceiveUIUpdate("selectcategory", SerializeObject(_competition), true);

                    UpdateParticipantsScores(_competitionId);
                    break;
                case "waitingroom":
                    if (!Context.User.IsInRole("Admin"))
                    {
                        await Clients.Caller.ReceiveUIUpdate("waitingroom", clientList.ToList(), true);
                    }
                    else
                    {
                        await Clients.Caller.ReceiveUIUpdate("adminwaitingroom", clientList.ToList(), true);
                    }
                    break;
                default:
                    if (Context.User.IsInRole("Admin"))
                    {
                        await Clients.Caller.ReceiveUIUpdate("adminwaitingroom", clientList.ToList(), true);
                    }
                    else
                    {
                        await Clients.Caller.ReceiveUIUpdate("setusername", new object() { }, true);
                    }
                    break;
            }
        }

        public async Task GetLastWinner()
        {
           var lastWinners=  _context.Scores.AsNoTracking()
               .Include(s => s.Participant)
               .AsEnumerable()
               .GroupBy(s => s.CompetitionId)
               .Take(10)
               .Select(g => g.OrderByDescending(s => s.Points).FirstOrDefault())
               .Select(s => new NameAndScore
               {
                   Name = s.Participant.Username,
                   Points = s.Points
               })
               .ToList();

            await Clients.Caller.LastWinners(SerializeObject(lastWinners));
        }

        public async Task GetLeaders()
        {
            var leaders = await _context.Scores.AsNoTracking().Include(z => z.Participant).OrderByDescending(z => z.Points).Take(10).Select(z => new NameAndScore() { Name = z.Participant.Username, Points = z.Points }).ToListAsync();

            await Clients.Caller.Leaders(SerializeObject(leaders));
        }

        /*Admin Functions*/
        public async Task StartGame(int competitionId)
        {
            if (!Context.User.IsInRole("Admin"))
            {
                await Clients.Caller.Error("Bu işlem için yetkiniz yok.");
                return;
            }

            _competitionId = competitionId;

            _competition = await _context.Competitions
            .AsNoTracking()
            .Include(c => c.CompetitionCategories)
            .ThenInclude(cc => cc.Category)
            .Include(c => c.CompetitionQuestions)
            .ThenInclude(cq => cq.Question)
            .FirstOrDefaultAsync(c => c.Id == competitionId);

            try
            {
                _lastScreenName = "selectcategory";

                await Clients.All.StartGame(SerializeObject(_competition));

                await Clients.All.ReceiveUIUpdate("selectcategory", SerializeObject(_competition), true);

                UpdateParticipantsScores(competitionId);

                _competitionParticipants = (List<string>)clientList.Select(z => z.Key);

                _gameStarted = true;
            }
            catch (Exception e)
            {
                throw new Exception("Test");
            }
        }

        public async Task NextQuestion()
        {
            if (!Context.User.IsInRole("Admin"))
            {
                await Clients.Caller.Error("Bu işlem için yetkiniz yok.");
                return;
            }

            var competition = await _context.Competitions
              .AsNoTracking()
              .Include(c => c.CompetitionCategories)
              .ThenInclude(cc => cc.Category)
              .Include(c => c.CompetitionQuestions)
              .ThenInclude(cq => cq.Question)
              .FirstOrDefaultAsync(c => c.Id == _competitionId);

            await NotifyNextParticipant();

            _lastScreenName = "selectcategory";

            Thread.Sleep(1000);

            await Clients.Group("Participants").NextQuestion(SerializeObject(competition));
            await Clients.Group("Participants").ReceiveUIUpdate("selectcategory", SerializeObject(competition), true);
        }

        public async Task ShowResults(int competitionId)
        {
            if (!Context.User.IsInRole("Admin"))
            {
                await Clients.Caller.Error("Bu işlem için yetkiniz yok.");
                return;
            }

            var participantsScores = _context.Scores
                .AsNoTracking()
                .Include(s => s.Participant)
                .Where(z => z.CompetitionId == competitionId).ToList()
                .Select(s => new { s.Participant.Username, s.Points });

            participantQueue.TryPeek(out string nextParticipant);

            var participantsInQueue = new HashSet<string>(participantQueue);

            var filteredScores = participantsScores
                .Where(s => participantsInQueue.Contains(s.Username))
                .ToList();

            Thread.Sleep(1000);

            await Clients.Group("Participants").ReceiveUIUpdate("seeresults", SerializeObject(filteredScores), true);

            await EndCompetition();
        }

        public async Task EndCompetition()
        {
            var competition = await _context.Competitions
              .FirstOrDefaultAsync(c => c.Id == _competitionId);

            competition.isOver = true;

            await _context.SaveChangesAsync();


            //RESET STATIC VARIABLES FOR NEXT COMPETITION
            participantQueue.Clear();
            clientList.Clear();
            clientList.Clear();
            categoryQuestions.Clear();
            activeQuestions.Clear();
            removedCategoryIds.Clear();

            _competitionId = 0;
            _questionId = 0;
            _gameStarted = false;
            _competitionParticipants.Clear();
            _lastScreenName = null;
            _lastSelectedQuestion = null;
            _competition = null;
    }

    /*Overrides*/
    public override async Task OnConnectedAsync()
        {
            var token = Context.GetHttpContext().Request.Query["token"];
            var id = Context.GetHttpContext().Request.Query["competitionId"];
            var username = Context.GetHttpContext().Request.Cookies["myUserName"];

            var isAdmin = Context.User.IsInRole("Admin"); 

            if (!await ValidateTokenAsync(token, id))
            {
                await Clients.Caller.InvalidToken();
                return;
            }

            if (_context.Competitions.AsNoTracking().Where(z=>z.isOver && z.Id == Int32.Parse(id.First())).FirstOrDefault() != null)
            {
                object participantsScores = null;

                try
                {
                    participantsScores = _context.Scores
                    .AsNoTracking()
                    .Include(s => s.Participant)
                    .Where(z => z.CompetitionId == Int32.Parse(id.First())).ToList()
                    .Select(s => new { s.Participant.Username, s.Points });
                }
                catch (Exception e)
                {
                    Console.Write(e.Message);
                }
              

                await Clients.Caller.ReceiveUIUpdate("seeresults", SerializeObject(participantsScores), true);
                await Clients.Caller.Disconnect();
            }
            else
            {
                if (isAdmin)
                {
                    adminList.TryAdd(Context.User.Identity.Name, Context.ConnectionId);

                    await Groups.AddToGroupAsync(Context.ConnectionId, "Participants");
                    await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
                    await Clients.Group("Admins").AdminJoined();

                    if (_lastScreenName == null)
                    {
                        _lastScreenName = "waitingroom";
                        await Clients.Caller.ReceiveUIUpdate("adminwaitingroom", clientList.ToList(), true);
                    }
                    else 
                    {
                        await GetLastScreen();
                    }
                }
                else
                {
                    if (username == null && (_lastScreenName == "waitingroom" || _lastScreenName == null))
                    {
                        await Clients.Caller.ReceiveUIUpdate("setusername", new object() { }, true);
                    }else if (username != null)
                    {
                        clientList.TryAdd(username, Context.ConnectionId);
                        participantQueue.Enqueue(username);

                        await Groups.AddToGroupAsync(Context.ConnectionId, "Participants");
                        await Clients.Caller.RecieveUserInfo(username, Context.ConnectionId);
                        await Clients.Group("Participants").UpdateParticipants(clientList.ToList());

                        await GetLastScreen();
                    }
                    else
                    {
                        await Clients.Caller.InvalidToken();
                        return;
                    }
                }
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            string connectionId = Context.ConnectionId;
            string userName = clientList.Where(entry => entry.Value == connectionId)
              .Select(entry => entry.Key).FirstOrDefault() ?? "";

            clientList.TryRemove(userName, out _);

            participantQueue = new ConcurrentQueue<string>(participantQueue.Where(s => s != userName));

            await Clients.Group("Participants").UpdateParticipants(clientList.ToList());

            await base.OnDisconnectedAsync(exception);
        }

       
        /*Helpers*/
        private async Task<bool> ValidateTokenAsync(string token, string competitionId)
        {
            if (token == null  || competitionId == null)
            {
                return false;
            }
 
            var competitionLink = await _context.Competitions.AsNoTracking().Where(z => z.Id == Int32.Parse(competitionId)).Select(z => z.JoinLink).FirstOrDefaultAsync();

            return competitionLink != null && competitionLink.IndexOf(token) != -1;
        }

        private async Task<bool> AreCompetitionQuestionsOver()
        {
            return _competition.CompetitionCategories.Count() == removedCategoryIds.Count(); ;
        }

        private string SerializeObject(object obj)
        {
            return JsonConvert.SerializeObject(obj, Formatting.None,
                        new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        });
        }
    }
}


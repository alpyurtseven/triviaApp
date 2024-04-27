using System.Data;
using Microsoft.AspNetCore.SignalR;
using triviaApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using Newtonsoft.Json;
using triviaApp.Utils;

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

        private readonly AppDbContext _context;

        public GameHub(AppDbContext context)
		{
            _context = context;
		}

        public async Task SetUsername(string username, int competitionId)
        {
            var connectionId = Context.ConnectionId;

            var existingParticipant = await _context.Participants
                .FirstOrDefaultAsync(p => p.Username == username && p.CompetitionId == competitionId);

            if (existingParticipant != null)
            {
                await Clients.Caller.Error("Bu kullanıcı adı bu yarışmada zaten kullanılmış.");
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

            await Clients.Group("Participants").UpdateParticipants(clientList.ToList());
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
                
                await Clients.Group("Participants").PresentQuestion(SerializeObject(question));

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

            await CheckAllAnswers(competitionId, questionId, false);
        }

        public async Task CheckAllAnswers(int competitionId, int questionId, bool timeOut)
        {
            var participantsInQueue = new HashSet<string>(participantQueue);

            var totalParticipants = await _context.Participants
                .Where(p => p.CompetitionId == competitionId && participantsInQueue.Contains(p.Username))
                .CountAsync();

            if (activeQuestions.Count >= totalParticipants || timeOut)
            {
                var correctAnswers = activeQuestions.Where(q => q.Value == questionId).Select(q => q.Key).ToList();
                var wrongAnswers = activeQuestions.Where(q => q.Value == -1).Select(q => q.Key).ToList();
                bool isCompetitionOver = await AreCompetitionQuestionsOver();
              
                Thread.Sleep(2000);

                await Clients.Group("Participants").QuestionResults(SerializeObject(correctAnswers), SerializeObject(wrongAnswers), isCompetitionOver);
                
                activeQuestions.Clear();
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

        /*Admin Functions*/
        public async Task StartGame(int competitionId)
        {
            if (!Context.User.IsInRole("Admin"))
            {
                await Clients.Caller.Error("Bu işlem için yetkiniz yok.");
                return;
            }

            _competitionId = competitionId;

            var competition = await _context.Competitions
            .AsNoTracking()
            .Include(c => c.CompetitionCategories)
            .ThenInclude(cc => cc.Category)
            .Include(c => c.CompetitionQuestions)
            .ThenInclude(cq => cq.Question)
            .FirstOrDefaultAsync(c => c.Id == competitionId);

            try
            {
                await Clients.All.StartGame(SerializeObject(competition));

                UpdateParticipantsScores(competitionId);
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

            Thread.Sleep(1000);

            await Clients.Group("Participants").NextQuestion(SerializeObject(competition));
        }


        /*Overrides*/
        public override async Task OnConnectedAsync()
        {
            var token = Context.GetHttpContext().Request.Query["token"];
            var id = Context.GetHttpContext().Request.Query["competitionId"];

            var isAdmin = Context.User.IsInRole("Admin"); 

            if (!await ValidateTokenAsync(token, id))
            {

                await Clients.Caller.InvalidToken();
                return;
            }

            if (isAdmin)
            {
                adminList.TryAdd(Context.User.Identity.Name, Context.ConnectionId);

                await Groups.AddToGroupAsync(Context.ConnectionId, "Participants");
                await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
                await Clients.Group("Admins").AdminJoined();
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            string connectionId = Context.ConnectionId;
            string userName = clientList.Where(entry => entry.Value == connectionId)
              .Select(entry => entry.Key).FirstOrDefault();

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
            var competition = await _context.Competitions
             .AsNoTracking()
             .Include(c => c.CompetitionCategories)
             .ThenInclude(cc => cc.Category)
             .Include(c => c.CompetitionQuestions)
             .ThenInclude(cq => cq.Question)
             .FirstOrDefaultAsync(c => c.Id == _competitionId);


            return competition.CompetitionCategories.Count() == removedCategoryIds.Count(); ;
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


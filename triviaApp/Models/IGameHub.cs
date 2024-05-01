using System;
using System.Collections.Concurrent;

namespace triviaApp.Models
{
	public interface IGameHub
	{
		Task JoinGame(string username, string connectionId);
		Task StartGame(string competition);
		Task EndGame();
        Task CategorySelected(string username, int categoryId);
        Task AnswerRecieved(bool isCorrect);
		Task NextQuestion(string competition);
        Task SetYourTurn(bool flag);
		Task PresentQuestion(string questionObject);
        Task InvalidToken();
		Task SendCategoriesThatHasNoQuestion(int categoryId);
		Task QuestionResults(string winners, string losers, bool competitionOver);
        Task Error(string message);
        Task EndTime();
        Task SetTime(string time);
        Task ReceiveParticipantsScores(string participantsScores, string nextParticipant);
        Task UpdateParticipants(List<KeyValuePair<string, string>> list);
        Task ReceiveUIUpdate(string screenName, object info, bool changeScreen);


        //ADMIN
        Task AdminJoined();
        Task UserAnswer(string username, string answer);
        Task QuestionTimeEnded();
    }
}


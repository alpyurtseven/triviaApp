window.competitionId = Number(window.location.search.split('competitionId=').slice(-1)[0].split('&')[0]);

/*HTML TEMPLATES*/

const getSelectCategoryTemplate = () => {
    return `<button class="btn btn-light selectCategory mt-3 w-100 justify-content-start mr-2" data-category-id="categoryid" isDisabledAttr>
                <span class="icon text-white-50">
                    <i class="fas fa-play"></i>
                </span>
                <span class="text">categoryname</span>
            </button>`;
}


const getAnswerTemplate = () => {
    return `<button class="btn btn-light answer mt-3 w-100 justify-content-start border border-dark mr-2" data-question-id="questionid" data-answer-text="answertextattr">
                <span class="text">answertextshow</span>
            </button>
            <div id="responseContainer" data-answer-text-show="answertextattrshow" class="mt-2 d-flex" style="min-height:25px;max-height:25px;"></div>`;
}


/*SingalR Events*/

const connectionBuild = () => {
    window.connection = new signalR.HubConnectionBuilder()
        .withUrl(window.location.origin + "/game" + window.location.search)
        .withAutomaticReconnect([1000, 1000, 2000, 3000, 5000, 10000, 30000])
        .build();


    startConnection();

    window.connection.onreconnecting(() => {
        $('.connectionAlert').remove();
        $('body').prepend(`<div class="alert alert-info mb-0 connectionAlert" role="alert">
                   Yeniden bağlantı kurulmaya çalışılıyor.
         </div>`);
    });

    window.connection.onreconnected(() => {
        $('.connectionAlert').remove();
        $('body').prepend(`<div class="alert alert-success mb-0 connectionAlert" role="alert">
                Bağlantı yeniden kuruldu.
         </div>`);

        setTimeout(() => { $('.connectionAlert').remove(); }, 2000)
    });

    window.connection.onclose(() => {
        $('.connectionAlert').remove();
        $('body').prepend(`<div class="alert alert-danger mb-0 connectionAlert" role="alert">
               Bağlantı hatası.
         </div>`);
    });
}

const startConnection = async () => {
    try {
        window.connection.start().then(function () {
            window.localStorage.removeItem('participants');

            onInvalidLink();
            onJoinGame();
            onUpdateParticipants();
            onStartGame();
            onCategorySelected();
            onReceiveParticipantsScores();
            onError();
            onSetYourTurn();
            onPresentQuestion();
            onSendCategoriesThatHasNoQuestion();
            onSetTime();
            onEndTime();
            onQuestionResults();
            onNextQuestion();
            onAdminJoined();
            onUserAnswer()
            onQuestionTimeEnded();
        });
    } catch {
        setTimeout(() => startConnection(), 2000);
    }
}

const joinGame = (username) => {
    window.connection.invoke("SetUsername", username, window.competitionId).then((response) => {
        $('.setusernamescreen').remove();
        $('.waitingroomscreen').addClass('d-flex');
        $('.waitingroomscreen').removeClass('d-none');
    }).catch(function (err) {
        return console.error(err.toString());
    });
}

const UpdateParticipantsScores = () => {
    window.connection.invoke("UpdateParticipantsScores", window.competitionId).then((response) => {
       
    }).catch(function (err) {
        return console.error(err.toString());
    });
}

const answerQuestion = (username, questionid, answer, competitionId) => {
    window.connection.invoke("AnswerQuestion", username, questionid, answer, competitionId).then((response) => {

    }).catch(function (err) {
        return console.error(err.toString());
    });
}

const onAdminJoined = () => {
    connection.on("AdminJoined", () => {
        $('.setusernamescreen').remove();
        $('.waitingroomscreen-admin').addClass('d-flex');
        $('.waitingroomscreen-admin').removeClass('d-none');

        $('.onlyAdmin').removeClass('d-none');

        window.isAdmin = true;
    });
}

const onJoinGame = () => {
    connection.on("JoinGame", (username, connectionId) => {
        window.user = {
            username: username,
            connectionId: connectionId
        }
    });
}

const onInvalidLink = () => {
    connection.on("InvalidToken", () => {
        window.location.replace('/app/invalidlink')
    });
}

const onStartGame = () => {
    connection.on("StartGame", (competition) => {
        $('.waitingroomscreen').remove();
        $('.waitingroomscreen-admin').remove();

        removeParticipantsListBackgrounds();

        window.localStorage.removeItem('categoriesThatHasNoQuestion');

        showCategorySelectionScreen(competition);
        setParticipants(JSON.parse(window.localStorage.getItem('participants')))
    });
}

const onUpdateParticipants= () => {
    connection.on("UpdateParticipants", (participants) => {
        window.localStorage.setItem('participants', JSON.stringify(participants));

        setParticipants(participants);
    });
}

const onError = () => {
    connection.on("Error", (message) => {
        $('.connectionAlert').remove();
        $('body').prepend(`<div class="alert alert-danger mb-0 connectionAlert" role="alert">
               ${ message }
         </div>`);

        setTimeout(() => { $('.connectionAlert').remove(); }, 1000);
    });
}

const onSetYourTurn = () => {
    connection.on("SetYourTurn", (status) => {
        if (status) {
            $('.connectionAlert').remove();
            $('body').prepend(`<div class="alert alert-success mb-0 connectionAlert" role="alert">
               Kategori seçme sırası sizde.
              </div>`);

            setTimeout(() => { $('.connectionAlert').remove(); }, 1000);
        }
    });
}

const onQuestionResults = () => {
    connection.on("QuestionResults", (corrects, wrongs, isOver) => {
        corrects = JSON.parse(corrects) || [];
        wrongs = JSON.parse(wrongs) || [];
        answers = [];

        if (isOver) {
            $('.nextQuestionAwait .waitText').text("Yarışma tamamlandı.");
            $('.nextQuestionAwait .spinners').addClass("d-none");


            if (window.isAdmin) {
                $('.nextQuestion-admin').addClass('d-none')
                $('.seeResults-admin').removeClass('d-none');
            }
        } else if (!$('.seeResults-admin').hasClass('d-none')) {
            $('.seeResults-admin').addClass('d-none');
        }

        $('.countdown').css('visibility', 'hidden');
        $('.questionPoint').css('visibility', 'hidden');
        UpdateParticipantsScores();
        changeAnswersBackground();

        $('.nextQuestionAwait').removeClass('d-none');
        $('.nextQuestionAwait').addClass('d-flex');

       

        corrects.map((username) => {
            answers.push({
                name: username,
                answer: true
            });
        });

        wrongs.map((username) => {
            answers.push({
                name: username,
                answer: false
            });
        });


        answers.map((answerObject) => {
            if (answerObject.answer) {
                $('[data-participant-name="' + answerObject.name + '"]').closest('li').addClass('bg-success');
                $('[data-participant-name="' + answerObject.name + '"]').closest('li').addClass('text-white');
            } else {
                $('[data-participant-name="' + answerObject.name + '"]').closest('li').addClass('bg-danger');
                $('[data-participant-name="' + answerObject.name + '"]').closest('li').addClass('text-white');
            }
        });
    });
}

const onPresentQuestion = () => {
    connection.on("PresentQuestion", (question) => {
        removeParticipantsListBackgrounds();
        $('.countdown').css('visibility', 'visible');
        $('.questionPoint').css('visibility', 'visible');

        $('.nextQuestionAwait').addClass('d-none');
        $('.nextQuestionAwait').removeClass('d-flex');

        const templates = [];

        question = JSON.parse(question || '{}') || {};

        window.ra = btoa(encodeURIComponent(question.RightAnswer));

        $('.questionbody span').text(question.Body);

        templates.push(getAnswerTemplate().replace('questionid', question.Id).replace('answertextattr', question.RightAnswer).replace('answertextshow', question.RightAnswer).replace('answertextattrshow', question.RightAnswer));


        for (let i = 0; i < 3; i++) {
            templates.push(getAnswerTemplate().replace('questionid', question.Id).replace('answertextattr', question[`WrongAnswer${i + 1}`]).replace('answertextshow', question[`WrongAnswer${i + 1}`]).replace('answertextattrshow', question[`WrongAnswer${i + 1}`]));
        }


        $('.answerSection-1 >').remove();
        $('.answerSection-2 >').remove();

        shuffle(templates).map((answer, index) => {
            if (index < 2) {
                $('.answerSection-1').append($(answer));
            } else {
                $('.answerSection-2').append($(answer));
            }
        });


        $('.categoryselectionscreen').addClass('d-none');
        $('.categoryselectionscreen').removeClass('d-flex');

        $('.questionPoint span').text('Puan Değeri: ' + question.Points);

        UpdateParticipantsScores();
        bindAnswerClick();

        setTimeout(() => {
            $('.q-screen').addClass('d-flex');
            $('.q-screen').removeClass('d-none');
        }, 100);
    });
}

const onUserAnswer = () => {
    connection.on("UserAnswer", (username, answer) => {
        const colors = ['bg-success', 'bg-warning', 'bg-danger'];
        const responseContainer = $('#responseContainer[data-answer-text-show="' + answer + '"]')[0];

        $(responseContainer).children().each((index, child) => {
            $(child)[0].className.split(' ').map((className) => {
                if (className.indexOf('bg-') === 0) {
                    const index = colors.indexOf(className);
                    if (index > -1) {
                        colors.splice(index, 1);
                    }
                }
            });
        })

        const responseDiv = document.createElement('div');

        responseDiv.textContent = username;
        responseDiv.className = `response ${colors[0]} flex-fill text-white d-flex align-items-center justify-content-center`;
        responseContainer.appendChild(responseDiv);
    });
}

const onSetTime = () => {
    connection.on("SetTime", (time) => {
        $('.countdown').text(time);
    });
}

const onEndTime = () => {
    connection.on("EndTime", (time) => {
        $('.countdown').text('0');
    });
}

const onSendCategoriesThatHasNoQuestion = () => {
    connection.on("SendCategoriesThatHasNoQuestion", (categoryId) => {
        const categoriesThatHasNoQuestion = JSON.parse(localStorage.getItem('categoriesThatHasNoQuestion') || '[]') || [];

        categoriesThatHasNoQuestion.push(categoryId);

        localStorage.setItem('categoriesThatHasNoQuestion', JSON.stringify(categoriesThatHasNoQuestion));
    });
}

const onNextQuestion = () => {
    connection.on("NextQuestion", (competition) => {
        $('.q-screen').addClass('d-none');

        showCategorySelectionScreen(competition);

        UpdateParticipantsScores();
    });
};

const onCategorySelected = () => {
    connection.on("CategorySelected", (id) => {
        $('.selectCategory[data-category-id="' + id + '"]').addClass('btn-success');
        $('.selectCategory[data-category-id="' + id + '"]').removeClass('btn-light');
    });
}

const onQuestionTimeEnded = () => {
    connection.on("QuestionTimeEnded", () => {
        checkAllAnswers();
    });
}

const onReceiveParticipantsScores = () => {
    connection.on("ReceiveParticipantsScores", function (participantsScores, nextParticipant) {
        var firstVisit = false;

        if ($('.participantsandScores span[data-participant-name]').length === 0) {
            $('.participantsandScores li').remove();

            firstVisit = true;
        } else {
            firstVisit = false;
        }
       

        $('.participantsandScores').each((index, scoresDiv) => {
            JSON.parse(participantsScores).forEach(score => {
                if (firstVisit) {
                    scoresDiv.innerHTML += ` <li class="list-group-item border-right"><div class="d-flex flex-column"><span data-participant-name="${score.Username}">Yarışmacı Adı: ${score.Username} </span><span>Puan:  ${score.Points}</span></div></li>`;
                } else {
                    $('[data-participant-name="' + score.Username + '"]').closest('li').find('span:last-child').text(`Puan: ${score.Points}`);
                }
            });

            if ($('.categoryselectionscreen').hasClass('d-flex')) {
                removeParticipantsListBackgrounds();

                $('[data-participant-name="' + nextParticipant + '"]').closest('li').addClass('bg-info');
                $('[data-participant-name="' + nextParticipant + '"]').closest('li').addClass('text-white');
            }
        });
    });
}

const selectCategory = (categoryId) => {
    window.connection.invoke("SelectCategory", Number(categoryId), competitionId, user.username).then((response) => {
       
    }).catch(function (err) {
        return console.error(err.toString());
    });
}

const setParticipants = (participants) => {
    const liTemplate = `<li class="list-group-item">username</li>`

    $('ul.participants li').remove();

    participants.map((participant) => {
        participant.key !== '' && $('ul.participants').append(liTemplate.replace('username', participant.key))
    });
}

const startGame = () => {
    window.connection.invoke("StartGame", window.competitionId).then((response) => {
        console.log('Competition Started');
    }).catch(function (err) {
        return console.error(err.toString());
    });
}

const checkAllAnswers = () => {
    window.connection.invoke("CheckAllAnswers", window.competitionId, Number($('[data-question-id]').attr('data-question-id')),true).then((response) => {
     
    }).catch(function (err) {
        return console.error(err.toString());
    });
}

const nextQuestion = () => {
    window.connection.invoke("NextQuestion").then((response) => {
        console.log('Next question invoked');
    }).catch(function (err) {
        return console.error(err.toString());
    });
}

/*DOM EVENTS*/

const bindSelectCategoryClick = () => {
    $('.selectCategory').on('click', (e) => {
        typeof selectCategory === 'function' && selectCategory(Number($(e.currentTarget).attr('data-category-id')));
    });
}

const bindAnswerClick = () => {
    $('.answer').on('click', (e) => {
        $(e.currentTarget).css('background-color', '#FFA500');
        $(e.currentTarget).removeClass('border-dark');
        $(e.currentTarget).addClass('border-danger');

        $(e.currentTarget).addClass('selectedAnswer');

        removeAnswerClick();

        typeof answerQuestion === 'function' && answerQuestion(window.user.username, Number($(e.currentTarget).attr('data-question-id')), $(e.currentTarget).attr('data-answer-text'), window.competitionId);
    });
}

const removeAnswerClick = () => {
    $('.answer').off('click');
}

const changeAnswersBackground = (rightAnswer) => {
    $('.answer').each((index, item) => {
        if ($(item).attr('data-answer-text') === decodeURIComponent(atob(window.ra))) {
            $(item).css('background-color', '#008000');
            $(item).removeClass('border-dark');
            $(item).removeClass('border-danger');
            $(item).addClass('border-light');

            if (!$(item).hasClass('selectedAnswer')) {
                $('.selectedAnswer').css('background-color', '#EE4B2B')
            }
        }
    });
}

const removeParticipantsListBackgrounds = (check) => {
    $('.participantsandScores li').removeClass('bg-success');
    $('.participantsandScores li').removeClass('bg-danger');
    $('.participantsandScores li').removeClass('bg-info');
    $('.participantsandScores li').removeClass('text-white');
}

const showCategorySelectionScreen = (competition) => {
    var parsedCompetition = JSON.parse(competition) || {};
    const categoriesThatHasNoQuestion = JSON.parse(localStorage.getItem('categoriesThatHasNoQuestion') || '[]') || [];

    $('.categorySelectionList').children().remove();


    parsedCompetition.CompetitionCategories.map((competitionCategory) => {
        selectCategoryTemplate = getSelectCategoryTemplate();

        if (categoriesThatHasNoQuestion.includes(competitionCategory.Category.Id)) {
            selectCategoryTemplate = selectCategoryTemplate.replace('isDisabledAttr', 'disabled');
        }

        selectCategoryTemplate = selectCategoryTemplate.replace('categoryname', competitionCategory.Category.Name);
        selectCategoryTemplate = selectCategoryTemplate.replace('categoryid', competitionCategory.Category.Id);

        $('.categorySelectionList').append($(selectCategoryTemplate));
    });

    $('.categoryselectionscreen').addClass('d-flex');
    $('.categoryselectionscreen').removeClass('d-none');

    bindSelectCategoryClick();
}

$(document).ready(() => {
    if (window.location.pathname.toLowerCase().includes('app/join')) {
        setTimeout(() => {
            connectionBuild();
        },50)
    }

    $('.startCompetition-admin').on('click', () => {
        typeof startGame === 'function' && startGame();
    });

    $('.nextQuestion-admin').on('click', () => {
        typeof nextQuestion === 'function' && nextQuestion();
    });
});

const shuffle = (array) => {
    let currentIndex = array.length;

    // While there remain elements to shuffle...
    while (currentIndex != 0) {

        // Pick a remaining element...
        let randomIndex = Math.floor(Math.random() * currentIndex);
        currentIndex--;

        // And swap it with the current element.
        [array[currentIndex], array[randomIndex]] = [
            array[randomIndex], array[currentIndex]];
    }

    return array;
}



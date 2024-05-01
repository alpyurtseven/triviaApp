window.competitionId = Number(window.location.search.split('competitionId=').slice(-1)[0].split('&')[0]);

/*HTML TEMPLATES*/
const getLoader = () => {
    return `<div bis_skin_checked="1">
        <div class="spinner-grow text-primary" role="status" bis_skin_checked="1">
            <span class="visually-hidden"></span>
        </div>
        <div class="spinner-grow text-secondary" role="status" bis_skin_checked="1">
            <span class="visually-hidden"></span>
        </div>
        <div class="spinner-grow text-success" role="status" bis_skin_checked="1">
            <span class="visually-hidden"></span>
        </div>
        <div class="spinner-grow text-danger" role="status" bis_skin_checked="1">
            <span class="visually-hidden"></span>
        </div>
        <div class="spinner-grow text-warning" role="status" bis_skin_checked="1">
            <span class="visually-hidden"></span>
        </div>
        <div class="spinner-grow text-info" role="status" bis_skin_checked="1">
            <span class="visually-hidden"></span>
        </div>
        <div class="spinner-grow text-primary" role="status" bis_skin_checked="1">
            <span class="visually-hidden"></span>
        </div>
        <div class="spinner-grow text-dark" role="status" bis_skin_checked="1">
            <span class="visually-hidden"></span>
        </div>
    </div>`;
}

const getSetUserNameScreen = () => {
    return `<div class="setusernamescreen">
        <div class="d-flex flex-column justify-content-center align-items-center">
            <h1>Tonbalıklımakarna Trivia App</h1>
            <hr />

            <div class="form-group d-flex flex-column">
                <input name="Username" id="username" class="form-control" placeholder="Kullanıcı adı" />

                <button class="btn btn-success startCompetition btn-icon-split mt-3 w-100 justify-content-start">
                    <span class="icon text-white-50">
                        <i class="fas fa-play"></i>
                    </span>
                    <span class="text">Yarışmaya Katıl</span>
                </button>

                <button class="btn btn-info btn-icon-split mt-3 w-100 justify-content-start">
                    <span class="icon text-white-50">
                        <i class="fas fa-trophy"></i>
                    </span>
                    <span class="text">Son Kazananlar</span>
                </button>

                <button class="btn btn-warning btn-icon-split mt-3 w-100 justify-content-start">
                    <span class="icon text-white-50">
                        <i class="fas fa-crown"></i>
                    </span>
                    <span class="text">En yüksek skorlar</span>
                </button>
            </div>

        </div>
    </div>`;
};

const getWaitingRoomScreen = (participantList) => {
    const participants = !(participantList || []).length ? '' : participantList.map((participant) => {
        return `<li class="list-group-item">${participant.key}</li>`;
    });

    return `<div class="waitingroomscreen">
     <div class="d-flex flex-column justify-content-center align-items-center">
            <h1>Tonbalıklımakarna Trivia App</h1>
            <hr />

            <div class="d-flex flex-column justify-content-center align-items-center">
                <h6>Yarışmanın başlatılması bekleniyor.</h6>
                <div class="p-3">
                    <div class="spinner-grow text-primary" role="status">
                        <span class="visually-hidden">.</span>
                    </div>
                    <div class="spinner-grow text-secondary" role="status">
                        <span class="visually-hidden"></span>
                    </div>
                    <div class="spinner-grow text-success" role="status">
                        <span class="visually-hidden"></span>
                    </div>
                </div>
            </div>

            <div class="mt-5">
                <div class="card" style="width: 18rem;">
                    <ul class="list-group list-group-flush participants">
                        ${(participants || []).join('')}
                    </ul>
                    <div class="card-footer">
                        Bağlı kullanıcılar
                    </div>
                </div>
            </div>
        </div>
    </div>`;
};

const getCategorySelectionScreen = (competition) => {
    const parsedCompetition = JSON.parse(competition) || {};
    const categoriesThatHasNoQuestion = JSON.parse(localStorage.getItem('categoriesThatHasNoQuestion') || '[]') || [];
    const categories = parsedCompetition.CompetitionCategories.map((competitionCategory) => {
        selectCategoryTemplate = getSelectCategoryTemplate();

        if (categoriesThatHasNoQuestion.includes(competitionCategory.Category.Id)) {
            selectCategoryTemplate = selectCategoryTemplate.replace('isDisabledAttr', 'disabled');
        }

        selectCategoryTemplate = selectCategoryTemplate.replace('categoryname', competitionCategory.Category.Name);
        selectCategoryTemplate = selectCategoryTemplate.replace('categoryid', competitionCategory.Category.Id);

        return selectCategoryTemplate;
    });


    return `<div class="categoryselectionscreen">
        <div class="mt-5 mb-5">
            <div class="card">
                <ul class="d-flex flex-row list-group list-group-flush participantsandScores">
                    <li class="list-group-item"><div class="d-flex flex-column"><span>Yarışmacı Adı: username </span><span>Puan: 0</span></div></li>
                    <li class="list-group-item"><div class="d-flex flex-column"><span>Yarışmacı Adı: username </span><span>Puan: 0</span></div></li>
                    <li class="list-group-item"><div class="d-flex flex-column"><span>Yarışmacı Adı: username </span><span>Puan: 0</span></div></li>
                </ul>
                <div class="card-footer text-center align-items-center justify-content-center d-flex">
                    Yarışmacılar
                </div>
            </div>
        </div>

        <div class="d-flex flex-column justify-content-center align-items-center">
            <div class="d-flex flex-column">
                <h1>Kategori seçiniz.</h1>

                <div class="mt-5 mb-5 d-flex flex-row categorySelectionList">
                   ${(categories || []).join('')}
                </div>
            </div>
        </div>
    </div>`;
};

const getQuestionScreen = (question) => {
    let templates = [];

    question = JSON.parse(question || '{}') || {};

    window.ra = btoa(encodeURIComponent(question.RightAnswer));

    templates.push(getAnswerTemplate().replace('questionid', question.Id).replace('answertextattr', question.RightAnswer).replace('answertextshow', question.RightAnswer).replace('answertextattrshow', question.RightAnswer));

    for (let i = 0; i < 3; i++) {
        templates.push(getAnswerTemplate().replace('questionid', question.Id).replace('answertextattr', question[`WrongAnswer${i + 1}`]).replace('answertextshow', question[`WrongAnswer${i + 1}`]).replace('answertextattrshow', question[`WrongAnswer${i + 1}`]));
    }


    templates = shuffle(templates);

    return `<div class="q-screen">
        <div class="mt-5 mb-5 mt-5 mb-5 d-flex justify-content-center">
            <div class="card questionCard">
                <ul class="d-flex flex-row list-group list-group-flush participantsandScores">
                    <li class="list-group-item"><div class="d-flex flex-column"><span>Yarışmacı Adı: username </span><span>Puan: 0</span></div></li>
                    <li class="list-group-item"><div class="d-flex flex-column"><span>Yarışmacı Adı: username </span><span>Puan: 0</span></div></li>
                    <li class="list-group-item"><div class="d-flex flex-column"><span>Yarışmacı Adı: username </span><span>Puan: 0</span></div></li>
                </ul>
                <div class="card-footer text-center align-items-center justify-content-center d-flex">
                    Yarışmacılar
                </div>
            </div>
        </div>

        <div class="nextQuestionAwait d-none justify-content-center align-items-center flex-column">
            <h3 class="waitText">Yöneticinin yarışmayı devam ettirmesi bekleniyor.</h3>

            <div bis_skin_checked="1" class="spinners">
                <div class="spinner-grow text-primary" role="status" bis_skin_checked="1">
                    <span class="visually-hidden"></span>
                </div>
                <div class="spinner-grow text-secondary" role="status" bis_skin_checked="1">
                    <span class="visually-hidden"></span>
                </div>
                <div class="spinner-grow text-success" role="status" bis_skin_checked="1">
                    <span class="visually-hidden"></span>
                </div>
            </div>

            <div class="onlyAdmin mt-5 d-none justify-content-center align-items-center">
                <button class="btn btn-info btn-icon-split mt-3 w-100 justify-content-start nextQuestion-admin d-flex">
                    <span class="icon text-white-50">
                        <i class="fas fa-play"></i>
                    </span>
                    <span class="text">Sonraki soru</span>
                </button>
                <button class="btn btn-warning btn-icon-split mt-3 w-100 justify-content-start seeResults-admin d-none">
                    <span class="icon text-white-50">
                        <i class="fas fa-play"></i>
                    </span>
                    <span class="text">Sonuçları görüntüle</span>
                </button>
            </div>
        </div>

       

        <div class="d-flex flex-column justify-content-center align-items-center">
            <div class="d-flex flex-column">
                <div class="questionPoint mb-5">
                    <span>Puan Değeri:  ${question.Points}</span>
                </div>

                <div class="questionbody rounded border p-5" style="min-height:200px">
                    <span>${question.Body}</span>
                </div>

                <div class="mt-5 mb-5 d-flex flex-row questionAnswers">
                    <div class="answerSection-1 d-flex flex-column w-100">
                       ${templates.slice(0,2).join('')}
                    </div>
                    <div class="answerSection-2 d-flex flex-column w-100 ml-2">
                        ${templates.slice(2, 4).join('')}
                    </div>
                </div>
                <hr />
                <div class="countdown rounded-circle align-self-center text-align-center justify-content-center d-flex align-items-center border border-info w-10" style="height: 150px; width: 150px; font-size: 1.8rem">
                    ${question.Time}
                </div>
            </div>
        </div>
    </div>`;
};

const getAdminWaitingroomScreen = (participantList) => {
    const participants = !(participantList || []).length  ? '' : participantList.map((participant) => {
        return `<li class="list-group-item">${participant.key}</li>`;
    });

    return `<div class="waitingroomscreen-admin">
        <div class="d-flex flex-column justify-content-center align-items-center">
            <h1>Tonbalıklımakarna Trivia App</h1>
            <hr />

            <div class="d-flex flex-column justify-content-center align-items-center">
                <h6>Yarışmacıların katılması bekleniyor.</h6>
                <div class="p-3">
                    <div class="spinner-grow text-primary" role="status">
                        <span class="visually-hidden">.</span>
                    </div>
                    <div class="spinner-grow text-secondary" role="status">
                        <span class="visually-hidden"></span>
                    </div>
                    <div class="spinner-grow text-success" role="status">
                        <span class="visually-hidden"></span>
                    </div>
                </div>
            </div>

            <div class="mt-5">
                <div class="card" style="width: 18rem;">
                    <ul class="list-group list-group-flush participants">
                        ${(participants || []).join('')}
                    </ul>
                    <div class="card-footer">
                        Yarışmacılar
                    </div>
                </div>
            </div>

            <div class="mt-5">
                <button class="btn btn-success startCompetition-admin btn-icon-split mt-3 w-100 justify-content-start">
                    <span class="icon text-white-50">
                        <i class="fas fa-play"></i>
                    </span>
                    <span class="text">Yarışmayı Başlat</span>
                </button>
            </div>
        </div>
    </div>`
};

const getSeeResultsScreen = (results) => {
    const parsedResults = JSON.parse(results) || [];

    parsedResults.sort(function (a, b) {
        return b.Points - a.Points;
    });

    const resultsHtml = parsedResults.map((result, index) => {
        const winner = index === 0 ? 'bg-success' : ''
        const winnerIcon = index === 0 ? '<span class="fas fa-crown text-white d-flex justify-content-center"></span>' : '';

        return `<li class="list-group-item ${winner}"><div class="d-flex flex-column">${winnerIcon}<span>Yarışmacı Adı: ${result.Username} </span><span>Puan: ${result.Points}</span></div></li>`;
    });


    return `<div class="seeresultsscreen">
        <div class="mt-5 mb-5 d-flex justify-content-center align-items-center flex-column">
            <div class="card">
                <ul class="d-flex flex-column list-group list-group-flush participantsandScores">
                    ${resultsHtml.join('')}
                </ul>
                <div class="card-footer text-center align-items-center justify-content-center d-flex">
                    Yarışma Sıralaması
                </div>
            </div>
        </div>
    </div>`;
};

const updateAppState = (state, info) => {
    const mainContainer = document.getElementById('appContainer');

    switch (state) {
        case 'loader':
            document.cookie = 'currentState=loader;path=/';
            localStorage.setItem('currentState', 'loader');
            mainContainer.innerHTML = getLoader();
            break;
        case 'setusername':
            document.cookie = 'currentState=setusername;path=/';
            localStorage.setItem('currentState', 'setusername');
            mainContainer.innerHTML = getSetUserNameScreen();

            $('.startCompetition').off('click').on('click', () => {
                const username = ($('#username').val() || '').trim();

                typeof joinGame === 'function' && joinGame(username)
            });
            break;
        case 'selectcategory':
            document.cookie = 'currentState=selectcategory;path=/';
            localStorage.setItem('currentState', 'selectcategory');
            mainContainer.innerHTML = getCategorySelectionScreen(info);

            bindSelectCategoryClick();
            break;
        case 'waitingroom':
            document.cookie = 'currentState=waitingroom;path=/';
            localStorage.setItem('currentState', 'waitingroom');
            mainContainer.innerHTML = getWaitingRoomScreen(info);
            break;
        case 'question':
            document.cookie = 'currentState=question;path=/';
            localStorage.setItem('currentState', 'question');
            mainContainer.innerHTML = getQuestionScreen(info);

            removeParticipantsListBackgrounds();
            UpdateParticipantsScores();
            bindAnswerClick();
            break;
        case 'adminwaitingroom':
            document.cookie = 'currentState=adminwaitingroom;path=/';
            localStorage.setItem('currentState', 'adminwaitingroom');
            mainContainer.innerHTML = getAdminWaitingroomScreen(info);

            $('.startCompetition-admin').off('click').on('click', () => {
                typeof startGame === 'function' && startGame();
            });
            break;
        case 'seeresults':
            document.cookie = 'currentState=seeresults;path=/';
            localStorage.setItem('currentState', 'seeresults');
            mainContainer.innerHTML = getSeeResultsScreen(info);
            break;
    }
}

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
            onSendCategoriesThatHasNoQuestion();
            onSetTime();
            onEndTime();
            onQuestionResults();
            onNextQuestion();
            onAdminJoined();
            onUserAnswer()
            onQuestionTimeEnded();
            onReceiveUIUpdate();
        });
    } catch {
        setTimeout(() => startConnection(), 2000);
    }
}

const joinGame = (username) => {
    window.connection.invoke("SetUsername", username, window.competitionId).then((response) => {
        localStorage.setItem('myUserName', username);
        document.cookie = 'myUserName=' + username +';path=/';

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
        removeParticipantsListBackgrounds();

        window.localStorage.removeItem('categoriesThatHasNoQuestion');

        setParticipants(JSON.parse(window.localStorage.getItem('participants')))
    });
}

const onReceiveUIUpdate = () => {
    connection.on("ReceiveUIUpdate", (state, info, changeScreen) => {
        if (changeScreen) {
            updateAppState(state, info);
        } else if (localStorage.getItem('currentState') === state) {
            updateAppState(state, info);
        }
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
                $('.seeResults-admin').addClass('d-flex');
                $('.nextQuestion-admin').removeClass('d-flex');
                $('.nextQuestion-admin').addClass('d-none');
                $('.seeResults-admin').removeClass('d-none');
            }
        } else if (!$('.seeResults-admin').hasClass('d-none')) {
            $('.seeResults-admin').addClass('d-none');
        }

        $('.countdown').css('visibility', 'hidden');
        $('.questionPoint').css('visibility', 'hidden');
        UpdateParticipantsScores();
        changeAnswersBackground();

        if (window.isAdmin) {
            $('.onlyAdmin').addClass('d-flex')
            $('.onlyAdmin').removeClass('d-none');
        }

        $('.nextQuestionAwait').removeClass('d-none');
        $('.nextQuestionAwait').addClass('d-flex');

        $('.nextQuestion-admin').off('click').on('click', () => {
            typeof nextQuestion === 'function' && nextQuestion();
        });

        $('.seeResults-admin').off('click').on('click', () => {
            typeof showResults === 'function' && showResults();
        });

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
                    scoresDiv.innerHTML += `<li class="list-group-item border-right"><div class="d-flex flex-column"><span data-participant-name="${score.Username}">Yarışmacı Adı: ${score.Username} </span><span>Puan:  ${score.Points}</span></div></li>`;
                } else {
                    $('[data-participant-name="' + score.Username + '"]').closest('li').find('span:last-child').text(`Puan: ${score.Points}`);
                }
            });

            if (localStorage.getItem('currentState') === 'selectcategory') {
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
    window.connection.invoke("CheckAllAnswers", window.competitionId, Number($('[data-question-id]').attr('data-question-id')), true, false).then((response) => {
     
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


const showResults = () => {
    window.connection.invoke("ShowResults", window.competitionId).then((response) => {
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

$(document).ready(() => {
    if (window.location.pathname.toLowerCase().includes('app/join')) {
        setTimeout(() => {
            connectionBuild();
        },50)
    }
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
using Jam.Api.Models;
using Jam.Api.Models.Enums;
using Microsoft.AspNetCore.Identity;

namespace Jam.Api.DAL;

public static class DBInit
{
    public static async Task SeedAsync(IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateAsyncScope();

        var storyContext = scope.ServiceProvider.GetRequiredService<StoryDbContext>();
        var authContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AuthUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        //await authContext.Database.MigrateAsync();   // Identity tables
        //await storyContext.Database.MigrateAsync();  // Story tables

        // Reset AuthDb
        await authContext.Database.EnsureDeletedAsync();
        await authContext.Database.EnsureCreatedAsync();

        // Reset StoryDb
        await storyContext.Database.EnsureDeletedAsync();
        await storyContext.Database.EnsureCreatedAsync();

        if (!storyContext.Database.CanConnect() || !authContext.Database.CanConnect())
        {
            Console.WriteLine("Database not ready yet!");
            return;
        }

        // Ensure roles exist
        if (!await roleManager.RoleExistsAsync("Admin"))
            await roleManager.CreateAsync(new IdentityRole("Admin"));

        if (!await roleManager.RoleExistsAsync("User"))
            await roleManager.CreateAsync(new IdentityRole("User"));

        // Seed users (normal and admin)
        if (!userManager.Users.Any())
        {
            var users = new List<AuthUser>
            {
                new() {
                    UserName = "TheFlash",
                    Email = "flash@example.com",
                    EmailConfirmed = true
                },
                new() {
                    UserName = "Smithy",
                    Email = "emily_smith@gmail.com",
                    EmailConfirmed = true
                },
                new() {
                    UserName = "LexLuthor",
                    Email = "bob@gmail.com",
                    EmailConfirmed = true
                },
                new()
                {
                    UserName = "AdminUser",
                    Email = "admin@jamapp.com",
                    EmailConfirmed = true
                }
            };

            foreach (var user in users)
            {
                string password = "Password123!"; // default for normal users

                if (user.Email == "admin@jamapp.com")
                    password = "AdminPassword123!"; // special password for admin

                var result = await userManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    // Assign roles
                    if (user.Email == "admin@jamapp.com")
                        await userManager.AddToRoleAsync(user, "Admin");
                    else
                        await userManager.AddToRoleAsync(user, "User");
                }
                else
                {
                    Console.WriteLine($"Error creating user {user.Email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }

        // (Optional) ensure admin has correct role even if users already existed
        var adminUser = await userManager.FindByEmailAsync("admin@jamapp.com");
        if (adminUser != null && !await userManager.IsInRoleAsync(adminUser, "Admin"))
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }

        // Seed stories, different scenes, answer options, and playing sessions
        if (!storyContext.Stories.Any())
        {
            var barry = await userManager.FindByEmailAsync("flash@example.com");
            var emily = await userManager.FindByEmailAsync("emily_smith@gmail.com");
            var bob = await userManager.FindByEmailAsync("bob@gmail.com");

            var stories = new List<Story>
            {
                new Story
                {
                    Title = "De Tre Bukkene Bruse",
                    Description = "Lille bukk, Mellomste bukk, og Store bukk skal over en bro for å spise gress, men under broen bor et stygt og farlig troll som vil spise dem.",
                    DifficultyLevel = DifficultyLevel.Medium,
                    Accessibility = Accessibility.Public,
                    Played = 20,
                    Finished = 12,
                    Failed = 5,
                    Dnf = 3,
                    UserId = barry?.Id,
                },
                new Story
                {
                    Title = "Gutten Som Kappåt Med Trollet",
                    Description = "Askeladden, en ung bondegutt, går en dag ut for å hogge litt ved. På sin ferd møter han på et farlig troll i skogen.",
                    DifficultyLevel = DifficultyLevel.Hard,
                    Accessibility = Accessibility.Private,
                    Played = 17,
                    Finished = 10,
                    Failed = 6,
                    Dnf = 1,
                    Code = "A123B29C",
                    UserId = emily!.Id,
                },
            };
            storyContext.Stories.AddRange(stories);


            // --------------------------------------------------------------------------------------------
            // CREATING 'DE TRE BUKKENE BRUSE'
            // --------------------------------------------------------------------------------------------
            var intro = new IntroScene
            {
                IntroText = "Det var en gang tre bukkene bruse som skulle til seters for å gjøre seg fete. På veien lå en stor elv, og over den gikk en steinbro. Under broen bodde et troll.",
                Story = stories[0]
            };

            var questionScene1 = new QuestionScene
            {
                SceneText = "Først kom den minste bukken bruse, som trippet over broen. 'Hvem er det som tripper på min bro?' brølte trollet. 'Det er bare lille bukken bruse', sa bukken med sin tynne stemme. 'Jeg skal til seters for å gjøre meg fet.'",
                Question = "Hva er 2 + 2? Svar riktig for å hjelpe lille bukk over broen og vekk fra trollet i trygghet!",
                Story = stories[0]
            };

            var answerOptions1 = new List<AnswerOption>
            {
                new AnswerOption
                {
                    Answer = "4",
                    FeedbackText = "Lille bukk var smart. Han sa: 'Du bør ikke ta meg, vent litt, for etter meg kommer en som er mye større! Trollet lot lille bukk gå...",
                    IsCorrect = true,
                },
                new AnswerOption
                {
                    Answer = "3",
                    FeedbackText = "Å nei du svarte feil! Trollet reiste seg opp fra broen for å spise ham, men lille bukk rakk akkurat å skrike ut: 'IKKE TA MEG! Vent litt, for etter meg kommer en som er mye større! Trollet tvilte, men bestemte seg for å høre på den lille bukken...",
                    IsCorrect = false,
                },
                new AnswerOption
                {
                    Answer = "2",
                    FeedbackText = "Å nei du svarte feil! Trollet ble sint, men lille bukk rakk akkurat å skrike ut: 'IKKE TA MEG! Vent litt, for etter meg kommer en som er mye større! Trollet tvilte, men bestemte seg for å høre på den lille bukken...",
                    IsCorrect = false,
                },
                new AnswerOption
                {
                    Answer = "5",
                    FeedbackText = "Å nei du svarte feil! Trollet begynte å krabbe ut fra broen for å ta lille bukk, men lille bukk rakk akkurat å skrike ut: 'IKKE TA MEG! Vent litt, for etter meg kommer en som er mye større! Trollet tvilte, men bestemte seg for å høre på den lille bukken...",
                    IsCorrect = false,
                },
            };

            questionScene1.AnswerOptions = answerOptions1;

            var questionScene2 = new QuestionScene
            {
                SceneText = "Så kom den mellomste bukken bruse. 'Hvem er det som tramper på min bro?' brølte trollet. 'Det er bare mellomste bukken bruse,' sa han, med sin litt tykkere stemme. 'Jeg skal til seters for å gjøre meg fet, akkurat som lille bukk!",
                Question = "Hva er 4 + 8? Svar riktig for å hjelpe mellomste bukk over broen og vekk fra trollet!",
                Story = stories[0]
            };

            questionScene1.NextQuestionScene = questionScene2;

            var answerOptions2 = new List<AnswerOption>
            {
                new AnswerOption
                {
                    Answer = "12",
                    FeedbackText = "Mellomste bukk var like glup som lille buk. Han sa: 'Dumme troll! Hvorfor skal du ta meg? Han som kommer etter meg er jo mye større! Trollet hørte også på mellomste bukk og lot han gå...",
                    IsCorrect = true,
                },
                new AnswerOption
                {
                    Answer = "10",
                    FeedbackText = "Å nei du svarte feil! Trollet kastet seg ut fra broen og skulle akkurat til å gripe tak i ham. Men den mellomste bukken hoppet unna og ropte: 'NEI! Ikke ta meg! Vent litt, for etter meg kommer en som er enda større, nemlig Store bukk!' Trollet var veldig usikker, men hørte på mellomste bukk...",
                    IsCorrect = false,
                },
                new AnswerOption
                {
                    Answer = "11",
                    FeedbackText = "Å nei du svarte feil! Trollet gikk kjapt ut fra broen og hoppet opp til der mellomste bukk sto. Men mellomste bukk tryglet og ba: 'NEI NEI NEI! ikke ta meg, vær så snill! Vent litt, for etter meg kommer en som er enda større, nemlig Store bukk!' Trollet nølte, men tenkte at han kunne spare kreftene til store bukk og lot mellomste bukk gå...",
                    IsCorrect = false,
                },
                new AnswerOption
                {
                    Answer = "14",
                    FeedbackText = "Å nei du svarte feil! Trollet tok fram en stor stein og skulle akkurat til å kaste den på mellomste bukk, men mellomste bukk rakk akkurat å overtale trollet: 'NEI! Ikke skad meg! Du bør heller vente på bukken etter meg, for han er enda større' Trollet senket armen og krabbet under broen igjen...",
                    IsCorrect = false,
                },
            };

            questionScene2.AnswerOptions = answerOptions2;

            var questionScene3 = new QuestionScene
            {
                SceneText = "Til slutt kom Store bukk, som dundret over broen. 'Hvem er det som dundrer på min bro?' brølte trollet, som var sint og sulten. Denne gangen var ikke bukken redd. Han sa med sin dype stemme: 'DET ER STORE BUKKEN BRUSE!",
                Question = "Hva er 3 - 5? Svar riktig for å hjelpe store bukk over broen!",
                Story = stories[0]
            };

            questionScene2.NextQuestionScene = questionScene3;

            var answerOptions3 = new List<AnswerOption>
            {
                new AnswerOption
                {
                    Answer = "-2",
                    FeedbackText = "Store bukk var ikke som de andre. Han var modig og sterk, og han hadde ingen planer om å la seg spise. Han svarte bestemt: 'JEG KOMMER FOR Å STANGE DEG!'",
                    IsCorrect = true,
                },
                new AnswerOption
                {
                    Answer = "0",
                    FeedbackText = "Å nei du svarte feil! Nå hadde trollet fått nok! Han stormet ut fra broen, hoppet opp og kastet seg over store bukk!",
                    IsCorrect = false,
                },
                new AnswerOption
                {
                    Answer = "-3",
                    FeedbackText = "Å nei du svarte feil! Nå var trollet illsint! Han hoppet opp og kastet seg over store bukk!",
                    IsCorrect = false,
                },
                new AnswerOption
                {
                    Answer = "2",
                    FeedbackText = "Å nei du svarte feil! Nå var trollet så lei av lille og mellomste bukk, nå skulle han endelig ta store bukk. Han hoppet opp og kastet seg over store bukk!",
                    IsCorrect = false,
                },
            };

            questionScene3.AnswerOptions = answerOptions3;

            var questionScene4 = new QuestionScene
            {
                SceneText = "De kjempet lenge. Store bukk stanget til trollet med sine horn, mens trollet reiv av pelsen til store bukk! Hvordan ender dette?",
                Question = "Hva er 2 + 2 - 3 + 5? Svar riktig for å hjelpe store bukk i kampen mot trollet!",
                Story = stories[0]
            };

            questionScene3.NextQuestionScene = questionScene4;

            var answerOptions4 = new List<AnswerOption>
            {
                new AnswerOption
                {
                    Answer = "6",
                    FeedbackText = "YES! Du svarte riktig! Store bukk fikk sveivet hornerne rett inn i magen på trollet! 'AAAUUUUU!!!', skrek trollet.",
                    IsCorrect = true,
                },
                new AnswerOption
                {
                    Answer = "4",
                    FeedbackText = "Å nei du svarte feil! Trollet slo så hardt til store bukk at han nærmest mistet synet!!",
                    IsCorrect = false,
                },
                new AnswerOption
                {
                    Answer = "8",
                    FeedbackText = "Å nei du svarte feil! Trollet løfter opp store bukk og kastet han hardt ned i steinbroen!",
                    IsCorrect = false,
                },
                new AnswerOption
                {
                    Answer = "-2",
                    FeedbackText = "Å nei du svarte feil! Trollet kastet store bukk hardt inn i rekkverket på broen!",
                    IsCorrect = false,
                },
            };

            questionScene4.AnswerOptions = answerOptions4;

            var goodEnding = new EndingScene
            {
                EndingType = EndingType.Good,
                EndingText = "Til tross for en spennende kamp, klarte til slutt store bukk å stange trollet ned fra broen. Trollet falt ut i elva og slo seg skikkelig. Alle tre bukkene kom seg trygt over broen. De spiste og koste seg, og levde et lykkelig liv. De ble aldri mer plaget av trollet...",
                Story = stories[0]
            };

            var neutralEnding = new EndingScene
            {
                EndingType = EndingType.Neutral,
                EndingText = "Etter en lang kamp, så falt både Store bukk og trollet ut fra broen og skadet seg skikkelig. Store bukk var litt lettere en trollet og kom seg om sider opp og ut av elva. Trollet derimot ble tatt videre med elva. De tre bukkene fikk spist og ble mette, men så seg alltid over skuldrene for å vokte seg for trollet...",
                Story = stories[0]
            };

            var badEnding = new EndingScene
            {
                EndingType = EndingType.Bad,
                EndingText = "Store bukk klarte ikke å stange trollet ut i elva, og trollet spiste ham. Lille bukk og mellomste bukk levde i sorg, og turte aldri å gå tilbake over broen igjen. De måtte finne seg et nytt sted å spise.",
                Story = stories[0]
            };




            // --------------------------------------------------------------------------------------------
            // CREATING 'GUTTEN SOM KAPPÅT MED TROLLET'
            // --------------------------------------------------------------------------------------------

            var introAskeladden = new IntroScene
            {
                IntroText = "Det var en gang en bonde som hadde tre sønner, en av disse var Askeladden. Han bestemte seg en dag for å dra ut til den store skumle skogen for å hogge litt ved. Med seg på turen fikk han med en stor ostebit til niste fra sin mor. I denne skogen hadde askeladden hørt rykter om troll, så motet tok han med seg ut til skogen",
                Story = stories[1]
            };

            var questionScene1Askeladden = new QuestionScene
            {
                SceneText = "Askeladden hogget ikke i mer enn noen minutter før det store stygge trollet sto over han. “Dersom du hugger i min skog, skal jeg drepe deg!' sa trollet.",
                Question = "Hva er 4 + 7? Svar for å hjelpe askeladden i situasjonen med trollet...",
                Story = stories[1]
            };

            var answerOptions1Askeladden = new List<AnswerOption>
            {
                new AnswerOption
                {
                    Answer = "11",
                    FeedbackText = "YES! Du svarte riktig! Askeladden lot seg ikke skremme av trollet. “Tier du ikke still”, skrek han til trollet, “skal jeg klemme deg, som jeg klemmer denne steinen!”, sa askeladden i det han klemte osten sin i stykker.",
                    IsCorrect = true,
                },
                new AnswerOption
                {
                    Answer = "6",
                    FeedbackText = "Å nei du svarte feil! Trollet tok et skritt nærmere Askeladden og løftet øksen sin høyt i været. Askeladden måtte tenke raskt og ropte: “Vet du hva? Jeg kan klemme denne steinen hardere enn deg!”, for å prøve å imponere trollet og klemte til.",
                    IsCorrect = false,
                },
                new AnswerOption
                {
                    Answer = "8",
                    FeedbackText = "Å nei du svarte feil! Trollet brølte så høyt at Askeladden falt bakover i frykt. Trollet grep tak i den store kniven sin og sa: “Nå skal du få kjenne hvor skarp min kniv er!' Askeladden måtte handle raskt og skrek: “VENT! Før du dreper meg, se hvor sterk jeg er!”, og klemte osten til den nærmest smeltet i hånden hans.",
                    IsCorrect = false,
                },
                new AnswerOption
                {
                    Answer = "10",
                    FeedbackText = "Å nei du svarte feil! Trollet fikk et skittent, gulhvitt smil om munnen og sa: “Du er en dårlig hugger, og du er dårlig til å regne!” Trollet gjorde seg klar til å svinge øks sin. Askeladden fikk panikk og uten å tenke seg om kastet han osten sin rett i øyet på trollet, midt i blinken! Trollet tok seg til øyet og brølte “AAAUUUUU!”, askeladden sa han hadde mer på lager og skulle bare prøve seg!",
                    IsCorrect = false,
                },
            };

            questionScene1Askeladden.AnswerOptions = answerOptions1Askeladden;

            var questionScene2Askeladden = new QuestionScene
            {
                SceneText = "Trollet be helt satt ut og redd askeladden. “Nei, kjære spar meg”, sa trollet, “jeg skal hjelpe deg å hugge.” Og det gjorde trollet, der ble de stående i timesvis. Da det led mot kvelden, sa trollet: “Nå kan du følge meg hjem, det er nærmere til meg enn til deg.”",
                Question = "Hva er 5 + 4? Svar riktig for å komme deg vekk fra trollet.",
                Story = stories[1]
            };

            questionScene1Askeladden.NextQuestionScene = questionScene2Askeladden;

            var answerOptions2Askeladden = new List<AnswerOption>
            {
                new AnswerOption
                {
                    Answer = "9",
                    FeedbackText = "YES! Du svarte riktig! Askeladden svarte bestemt: “Nei, jeg skal hjem, og hvis du prøver å stoppe meg så bruker jeg kreftene mine på deg!” Men Askeladden tenkte seg litt om, han hadde jo tross alt hørt rykter om at trollet hadde masse gull og sølv hjemme i sin grotte. “Jeg har ombestemt meg, jeg kan følge deg hjem, men da skal du bære meg!”, sa Askeladden. Trollet gjorde sam han sa og puttet han i baklomma si.",
                    IsCorrect = true,
                },
                new AnswerOption
                {
                    Answer = "10",
                    FeedbackText = "Å nei du svarte feil! Trollet ble veldig irritert over det gale svaret ditt. “Nå er jeg lei av deg og alt tullet ditt!” brølte trollet, og ga Askeladden et kraftig slag i hodet så han ble halvveis bevisstløs. Trollet mumlet: “Du skal få se hvem som er sjef når vi kommer hjem”, og puttet Askeladden i baklomma si.",
                    IsCorrect = false,
                },
                new AnswerOption
                {
                    Answer = "8",
                    FeedbackText = "Å nei du svarte feil! Trollet så ondt på Askeladden. “Du er ikke så klok som du tror!”, sa trollet og tok tak i Askeladden, løftet ham opp i luften, og ristet ham hardt før han kastet ham ned i den romslige sekken sin. “Du har ikke noe valg, du skal hjem til meg”, sa trollet og begynte å gå.",
                    IsCorrect = false,
                },
                new AnswerOption
                {
                    Answer = "7",
                    FeedbackText = "Å nei du svarte feil! Trollet skjønte at Askeladden kanskje ikke var så tøff som han trodde. Trollet grep Askeladden i armen og tvang ham bort fra skogen. Trollet var for sterk, “Nå blir du med meg hjem, enten du vil eller ei!” sa trollet og stappet ham godt ned i lommen sin.",
                    IsCorrect = false,
                },
            };

            questionScene2Askeladden.AnswerOptions = answerOptions2Askeladden;

            var questionScene3Askeladden = new QuestionScene
            {
                SceneText = "Etter en del vandring kom de omsider frem til grotten til trollet. Både trollet og Askeladden var blitt sultne nå, så trollet kokte opp en dugelig stor grautgryte. De satte seg til bords, men før Askeladden rakk å hive i seg første sleiv med graut spurte trollet: “Vil du som jeg kappete?”",
                Question = "Hva er 8 + 13?",
                Story = stories[1]
            };

            questionScene2Askeladden.NextQuestionScene = questionScene3Askeladden;

            var answerOptions3Askeladden = new List<AnswerOption>
            {
                new AnswerOption
                {
                    Answer = "21",
                    FeedbackText = "YES! Du svarte riktig. Askeladden var ikke tung å be. “Å ja!” svarte han; for det tenkte han alltid han skulle stå seg i. Han som var smartere enn trollet skulle nok klare å få lurt han!",
                    IsCorrect = true,
                },
                new AnswerOption
                {
                    Answer = "19",
                    FeedbackText = "Å nei du svarte feil! Trollet lo høyt og tok seg til hode. “Du har ikke noe valg! Kappeting skal vi gjøre, og hvis du taper dreper jeg deg!”, sa trollet nifst.",
                    IsCorrect = false,
                },
                new AnswerOption
                {
                    Answer = "23",
                    FeedbackText = "Å nei du svarte feil! Trollet ble irritert og grep tak i Askeladden's arm. “Svarer du så dårlig på matte, er du neppe god til eting heller!” Trollet tvang Askeladden til kappeting.",
                    IsCorrect = false,
                },
                new AnswerOption
                {
                    Answer = "25",
                    FeedbackText = "Å nei du svarte feil! Trollet snøftet foraktelig og slo neven i bordet så det ristet. “Du er en tåpe! Spising er alvor!” Trollet hentet sin største tresleiv og sa: “Vi kappeter om livet ditt! Sett i gang å spis.”",
                    IsCorrect = false,
                },
            };

            questionScene3Askeladden.AnswerOptions = answerOptions3Askeladden;

            var questionScene4Askeladden = new QuestionScene
            {
                SceneText = "Så satte de i gang med kappetingen. Trollet heiv i seg sleiv etter sleiv og tittet iblant ned på Askeladden for å se om han klarte å holde følge.",
                Question = "Hva er 12 + 4 - 7? Svar riktig for å hjelpe askeladden i kappetingen.",
                Story = stories[1]
            };

            questionScene3Askeladden.NextQuestionScene = questionScene4Askeladden;

            var answerOptions4Askeladden = new List<AnswerOption>
            {
                new AnswerOption
                {
                    Answer = "9",
                    FeedbackText = "YES! Du svarte riktig. Askeladden var lur og tok på seg sekken sin så den hang foran magen hans. Mens trollet var opptatt med å hive i seg graut, helte Askeladden grauten fra sin sleiv ned i sekken, før han hevet sleiven opp mot munnen og latet som han spiste grauten.",
                    IsCorrect = true,
                },
                new AnswerOption
                {
                    Answer = "5",
                    FeedbackText = "Å nei du svarte feil! Askeladden kastet raskt på seg sekken sin. Trollet så ned, men var for opptatt med å spise til å se at Askeladden febrilsk helte sleiv på sleiv med grøt ned i sekken. Men Askeladden var stresset og sølte masse grøt. Dette fikk trollet med seg, “Fortsetter du å grise slikt hiver jeg den inn i ovnen!”, ropte trollet. Heldigvis oppdaget han ikke sekken full av graut, men det var så vidt!",
                    IsCorrect = false,
                },
                new AnswerOption
                {
                    Answer = "8",
                    FeedbackText = "Å nei du svarte feil! Askeladden grep tak i sekken for å helle graut ned i den, men akkurat da tittet trollet ned på han. Askeladden måtte kjapt lene seg frem for å skjule sekken, og heldgvis oppdaget ikke trollet den. Askeladden fortsatt å helde graut ned i sekken sin.",
                    IsCorrect = false,
                },
                new AnswerOption
                {
                    Answer = "11",
                    FeedbackText = "Å nei du svarte feil! Askeladden kastet raskt på seg sekken for å helle graut i den. Men grauten laget en slafsende lyd da den traff bunnen av sekken! Trollet stivnet, lyttet, og sa: “Hva var den lyden? Hva gjør du med grauten, du jukser vel ikke?” Askeladden ristet hektisk på hode og heiv raskt i seg noen svære sleiver med graut for å overbevise trollet at han spiste. Trollet fortsatte å ete, men holdt et ekstra godt øye med Askeladden.",
                    IsCorrect = false,
                },
            };

            questionScene4Askeladden.AnswerOptions = answerOptions4Askeladden;

            var questionScene5Askeladden = new QuestionScene
            {
                SceneText = "Etter en stund var sekken til Askeladden full av graut. Han tok fram spikkekniven sin, tittet opp på trollet, og skar hull i sekken så all grauten rant ut. Trollet, halveis blind som han var, trodde Askeladden hadde skåret hull på magesekken sin. Gjør du som jeg og skjærer hull på magen, så eter du så mye du vil.”, sa Askeladden. “Men det gjør vel gruelig vondt?” spurte trollet. “Å, ikke noe å tale om”, svarte Askeladden.",
                Question = "Hva er 3 - 5 + 3 - 2? Svar riktig for å overtale trollet til å skjære hull om magen sin!",
                Story = stories[1]
            };

            questionScene4Askeladden.NextQuestionScene = questionScene5Askeladden;

            var answerOptions5Askeladden = new List<AnswerOption>
            {
                new AnswerOption
                {
                    Answer = "-1",
                    FeedbackText = "YES! Du svarte riktig. Askeladden klarte å overtale trollet til å gjøre det samme. Trollet tok frem kniven, og skjærte til så både graut, blod og organer tøyt ut av magen hans!",
                    IsCorrect = true,
                },
                new AnswerOption
                {
                    Answer = "0",
                    FeedbackText = "Å nei du svarte feil! Trollet lente seg frem og studerte Askeladden nøye. “Det der er jo ikke magen din, det er sekken din!” brummet trollet. “Du har JUKSET!” Trollet tok et hardt grep om Askeladden og tvang ham til å skjære sin faktiske mage. Askeladden hadde ingen valg, han måtte skjære. Men nå som Askeladden var så nærme trollet klarte han å strekke seg til å sette kniven i trollets mage i stedet, og da slapp trolllet tak i Askeladden!",
                    IsCorrect = false,
                },
                new AnswerOption
                {
                    Answer = "1",
                    FeedbackText = "Å nei du svarte feil! Trollet ble mistenksom og lente seg fram for å kjenne på grauten: “Den er jo kald! Om grauten hadde vært inne i magen din hadde den jo fortsatt vært varm!”, ropte trollet. Han tok fram den svære skarpe kniven sin for å drepe Askeladden, men liten som han var klarte Askeladden å hoppe unna og skjære trollets mage! Trollet brølte høyt, graut og blod begynte å tyte ut av magen hans!",
                    IsCorrect = false,
                },
                new AnswerOption
                {
                    Answer = "-2",
                    FeedbackText = "Å nei du svarte feil! Trollet studerte grauten nøye. “Hvis denne grauten kom ut av magen din, hvorfor er det ikke noe blod her?”, spurte trollet. Askeladden fikk panikk og ante ikke hva han skulle svare. Men akkurat før trollet skulle til å gripe tak i han ropte Askeladden: “Fordi menneskeblod er ikke rødt som troll-blod! Det er lyst!”. Heldgvis trodde trollet på Askeladdens påstand og skjærte opp magen sin for å bli kvitt metthetsfølelsen!",
                    IsCorrect = false,
                },
            };

            questionScene5Askeladden.AnswerOptions = answerOptions5Askeladden;

            var goodEndingAskeladden = new EndingScene
            {
                EndingType = EndingType.Good,
                EndingText = "Trollet falt om og holdt seg rundt magen i smerte. Han prøvde krampaktig å stoppe blodet, men det bare rant ut, og vipps så var trollet dø. Askeladden tok alt det sølv og gull som var i grotten til trollet, og gikk hjem og levde lykkelig i alle hans dager.",
                Story = stories[1]
            };

            var neutralEndingAskeladden = new EndingScene
            {
                EndingType = EndingType.Neutral,
                EndingText = "Trollet falt om og skrek i smerte. Askeladden løp mot utgangen av grotten, men fikk øye på gull og sølv som lå i den store kisten. Han tok med seg det kan maktet å bære, men i det han skulle åpne den store tunge døren hadde trollet klart å åle seg bort til Askeladden og tok tak i foten hans. Askeladden måtte dessverre bruke mye av det harde gullet og sølvet til å slå trollet bevisstløs, men klarte akkurat å komme seg unna med noe av rikdommen.",
                Story = stories[1]
            };

            var badEndingAskeladden = new EndingScene
            {
                EndingType = EndingType.Bad,
                EndingText = "Trollet falt om og holdt seg rundt magen, men det var ikke over. I sitt siste, døende øyeblikk grep trollet tak i Askeladden med et voldsomt, krampaktig tak. Askeladden prøvde å rive seg løs, men trollet var for sterk. Han slengte Askeladden hardt i fjellveggen før han selv falt om igjen og døde. Askeladden ble hardt skadet og liggende alene. Han kom seg aldri ut av gråtten og hjem til familien...",
                Story = stories[1]
            };




            // Adding IntroScenes to the db
            storyContext.IntroScenes.AddRange(intro, introAskeladden);

            // Adding QuestionScenes to the db
            storyContext.QuestionScenes.AddRange(
                questionScene1, questionScene2, questionScene3, questionScene4,
                questionScene1Askeladden, questionScene2Askeladden, questionScene3Askeladden, questionScene4Askeladden, questionScene5Askeladden
            );

            // Adding EndingScenes to the db
            storyContext.EndingScenes.AddRange(
                goodEnding, neutralEnding, badEnding,
                goodEndingAskeladden, neutralEndingAskeladden, badEndingAskeladden
            );

            // Adding AnswerOptions for 'De Tre Bukkene Bruse' to the db
            storyContext.AnswerOptions.AddRange(
                answerOptions1.Concat(answerOptions2)
                              .Concat(answerOptions3)
                              .Concat(answerOptions4)
            );

            // Adding AnswerOptions for 'Gutten Som Kappåt Med Trollet' to the db
            storyContext.AnswerOptions.AddRange(
                answerOptions1.Concat(answerOptions2Askeladden)
                              .Concat(answerOptions3Askeladden)
                              .Concat(answerOptions4Askeladden)
                              .Concat(answerOptions5Askeladden)
            );

            await storyContext.SaveChangesAsync();

            // Creating some dummy-PlayingSessions
            var theFlashPlayingSession = new PlayingSession
            {
                StartTime = DateTime.Now.AddMinutes(-8),
                EndTime = DateTime.Now,
                Score = 18,
                MaxScore = 25,
                CurrentLevel = 3,
                CurrentSceneId = goodEndingAskeladden.EndingSceneId,
                CurrentSceneType = SceneType.Ending,
                Story = stories[1],
                UserId = barry!.Id,
            };
            var smithyPlayingSession = new PlayingSession
            {
                StartTime = DateTime.Now.AddMinutes(-15),
                Score = 5,
                MaxScore = 20,
                CurrentLevel = 3,
                CurrentSceneId = questionScene2.QuestionSceneId,
                CurrentSceneType = SceneType.Question,
                Story = stories[0],
                UserId = emily.Id,
            };

            storyContext.PlayingSessions.AddRange(theFlashPlayingSession, smithyPlayingSession);
            await storyContext.SaveChangesAsync();
        }
    }
}
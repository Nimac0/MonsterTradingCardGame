Die Datenbank muss unter "mydb" mit dem User "nimaco" Password "mtcgSwen" erreichbar sein,
das Schema kann mit dem beigefügtem create.sql Skript erstellt werden.

1.) Projektaufbau
-Db
-Http
-RequestHandler
-Schemas
-Fight

-Db-
Hier befindet sich die Klasse DbQuery und das Interface IDatabase, mit dieser ist es möglich
durch die Funktion newCommand(string sqlcommand) einen neuen Sql request zu erstellen,
mithilfe der Dbquery Instanz die sich bereits in den Klassen befindet in denen 
Sql requests gebraucht werden.

-Http-
In dem Http Ordner befinden sich jene Klassen die für den Http/REST Server zuständig
sind. Connection ist dafür da um mithilfe von sockets requests entgegennehmen zu können
und responses wieder zurück an den Client zu schicken. Diese Responses werden wiederum
durch die Klasse Response generiert, die die für den Client notwendige Daten und Response Codes
in das richtige Format bringen. MethodRouter ist dafür zuständig dass die Requests an die
richtigen Funktionen weitergeleitet werden und der SessionHandler ist für das Login und 
authorisieren von Requests zuständig.

-RequestHandler-
Der RequestHandler beinhaltet alle notwendigen Handler für die Requests des Client.
CardHandler schickt dem User seine Karten bzw. sein Deck, lässt den User das Deck
konfigurieren und erstellt einzelne Karten die dann wiederum im Packagehandler benötigt werden.
Der PackageHandler ermöglicht es packeges zu erstellen und zu kaufen.
Der TradeHandler ist
für das Erstellen, Löschen, Aufrufen und Durchführen eines Tausches zuständig, dabei wird
jeweils gecheckt ob ein Angebot die requirements des Tausches erfüllen und ob der User die
Angebotene Karte in erster Linie besitzt.
Der Fighthandler beinhaltet die lobby und ist für die Erstellung und Ausführung von Kämpfen zuständig.
Der UserHandler ermöglicht dem User seine Daten aufzurufen und zu verwalten, außerdem
ist er dafpr zuständig neue User zu erstellen.

-Schemas-
Diese sind praktisch Vorlagen für Objekte die entweder in der Datenbank persistiert werden oder
für die Art und Weise wie Daten zurück an den Client geschickt werden sollen.

-Fight-
Hier befindet sich die Hauptlogik des Spiels und die Berechnung des Gewinners eines Kampfes.
Desweiteren werden hier die Eloberechnung und das Unique Feature eingebaut.

-Unique Feature-
Wenn ein Spieler gewinnt, bekommt er zufällig eine der Karten vom Gegner, der Verlierer bekommt
einen coin damit es nicht möglich ist sich selbst durch zu viel verlieren permanent zu sperren.
(-> nach 5 mal verlieren hat der Spieler 5 Karten weniger -> kann sich ein neues package kaufen
um wieder neue Karten zu haben)


2.) Zeitaufwand
-Datenbank 4:40h
-FightLogic 8h
-Connection 2:30h
-MethodRouter + Parsen des requests 3:40h
-Response 1h
-DbQuery Klasse (+verworfenen Prototypen) 5:20h
-CardHandler 4:30h
-FightHandler 3:50h
-PackageHandler 6:10h
-TradeHandler 5:40h
-UserHandler 6:20h
-Card (+ rumprobieren bis das endgültige Schema gewählt wurde) 3:25h
-User -"- 2:50h
-alle anderen schemen zusammen ca 30 min
-Unittests 6:20h
-Weiteres neuschreiben/formattieren/debuggen/Testen/anpassen an curlscript 14:30h

3.) Unittests
Die Unittests sind unterteilt in
-FightTest
-RequestTest
-CardTest

Dabei wird gecheckt ob die Fightlogik richtig funktioniert, ob z.B. schlechte Requests
richtige ResponseCodes zurückschicken und ob Karten anhand der gegebenen Angaben korrekt
erstellt werden.
Diese drei Kategorien wurden gewählt da ich im Laufe des Programmierens gemerkt habe das dabei öfters
feher entstandne sind die im weiteren Verlauf des Programms zu Problemen und Errors geführt haben,
dehalb habe ich es für sinnvoll gehalten diese Teile zu testen um die Sucharbeit nach der Quelle
der Fehler zu minimieren. Außerdem ist vorallem der Fight das wo die meiste Logik stattfindet.

4.) Verlauf, Lessons learned
Zu Beginn habe ich mit der FightLogic angefangen und dabei nur die Text specification als guide benutzt.
-> hätte früher die api specification und curl script genauer beachten sollen da mir das
viel rewriting erspart hätte
-> ebenfalls hätte ich dadurch viel schneller eine konkretere Struktur meines Projekts gehabt
und viel rumprobieren erspart (aufgrund vergangener Projekte war ich mir nicht sicher wie viel
Freiraum ich bei dem definieren verschiedener Funktionen und schemen tatsächliche hätte, es war
im Endeffekt weniger als zuerst eingeschätzt)
-> ich habe von Anfang an Sockets für meine Connection benutzt habe dann alles versucht umzuschrieben weil
mir gesagt wurde dass Sockets komplizierter sind, habe mich im Endeffekt trotzdem für Sockets entschieden
da der mehraufwand im Vergleich zum TcpListener nicht viel mehr war und ich von einem anderen Fach noch
im Gedächtnis hatte wie Sockets funktionieren
-> SessionHandler und FightHandler wurden aufgrund der unter den Threads geteilten Informationen
(Lobby, Session Dictionary) zu Singletons gemacht um den Zugriff zu erleichtern und das gemeinsame starten
in einen Kampf trotz verschiedener Threads zu ermöglichen.
-> anfangs hatte ich die parent class card die sowohl an spellcard als auch an monstercard vererbt hat
habe mich im endeffekt aber dann nur für eine Klasse entschieden da die KartenTypen nichts bis auf den Enum Type
unterscheidet
-> Da das einzige Interface das ich habe das IDatabase Interface ist musst ich zum mocken einige
funktionen virtual machen
-> im nachhinein hätte ich geschaut ob es möglich gewesen wäre von Anfang an die Struktur der Handler zu definieren
und daraus ein Interface oder eine Parent Klasse zu machen.

Obwohl das Rumprobieren viel Zeit gekostet hat konnte ich dadurch einige C# und Web Server Prinzipien besser nachvollziehen.
Mehr Planung wäre hilfreich gewesen aber durch mein fehlendes C# Wissen zu Beginn konnte ich nicht ganz einschätzen
was alles eingeplant werden musste, weshalb ich einfach mal angefangen habe zu programmieren und dann im Endeffekt viel ändern musste.

 
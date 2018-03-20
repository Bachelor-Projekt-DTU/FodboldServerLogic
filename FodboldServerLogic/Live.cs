using FodboldApp.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using System.Linq;
using Realms;
using Realms.Sync;

namespace FodboldServerLogic
{
    class Live
    {
        static MatchModel newMatch = new MatchModel();
        static MatchModel match = new MatchModel();
        static string fileName = "live.xml";
        static string League = "Superligaen";
        static string ClubName = "SoenderjyskE";
        static ObservableCollection<MatchModel> matches = new ObservableCollection<MatchModel>();
        static ObservableCollection<EventModel> events = new ObservableCollection<EventModel>();
        static ObservableCollection<CommentModel> comments = new ObservableCollection<CommentModel>();
        static int prevScore1 = 0;
        static int prevScore2 = 0;
        static int listSize1 = 0;
        static int listSize2 = 0;
        static string[] teams;
        static Realm _realm = null;

        static void Main(string[] args)
        {
                    var user = User.LoginAsync(Credentials.UsernamePassword("realm-admin", "bachelor", false), new Uri($"http://13.59.205.12:9080")).Result;
                    SyncConfiguration config = new SyncConfiguration(user, new Uri($"realm://13.59.205.12:9080/data/matches"));
                    _realm = Realm.GetInstance(config);
            while (true)
            {
                CheckFile();
                Thread.Sleep(1000);
            }
        }

        static void CheckFile()
        {
            try
            {
                if(_realm == null)
                {
                }
                XDocument doc = XDocument.Load(fileName);

                foreach (XElement el in doc.Root.Elements())
                {
                    if (el.Attribute("league").Value == League)
                    {
                        foreach (XElement XMatchData in el.Elements())
                        {
                            var s0 = XMatchData.Attribute("name").Value.Split("-");
                            if (s0[0] == ClubName || s0[1] == ClubName)
                            {
                                MatchModel temp = _realm.Find<MatchModel>(XMatchData.Attribute("id").Value);
                                teams = XMatchData.Attribute("name").Value.Split("-");
                                int i = 0;
                                foreach (XElement result in XMatchData.Elements("results"))
                                {
                                    if (i++ == 0)
                                    {
                                        int x = Int32.Parse(result.Element("result").Attribute("value").Value);
                                        if (prevScore1 != x)
                                        {
                                            prevScore1 = x;
                                            //send it
                                            temp.Score1 = x;
                                            Console.WriteLine("team 1 goal");
                                        }
                                    }
                                    else
                                    {
                                        int x = Int32.Parse(result.Element("result").Attribute("value").Value);
                                        if (prevScore2 != x)
                                        {
                                            prevScore2 = x;
                                            //send it
                                            temp.Score2 = x;
                                            Console.WriteLine("team 2 goal");
                                        }
                                    }
                                }
                                i = 0;
                                foreach (XElement matchEvent in XMatchData.Elements("incidents"))
                                {
                                    if (matchEvent.HasElements)
                                    {
                                        events.Clear();
                                        foreach (XElement innerEvent in matchEvent.Elements("incident"))
                                        {
                                            EventModel evtemp = new EventModel { MatchId = temp.Id, PlayerName = innerEvent.Attribute("player").Value, Team = i, Type = innerEvent.Attribute("type").Value };
                                            events.Add(evtemp);
                                        }
                                        if (i++ == 0)
                                        {
                                            if (events.Count != listSize1)
                                            {
                                                listSize1 = events.Count;
                                                //send based on last
                                                SendNotification();
                                                Console.WriteLine("team 1 holla");
                                            }
                                        }
                                        else
                                        {
                                            if (events.Count != listSize2)
                                            {
                                                listSize2 = events.Count;
                                                //send based on last
                                                SendNotification();
                                                Console.WriteLine("team 2 holla");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    //foreach (XElement el in doc.Root.Elements())
                    //{

                    //    int i = 0;
                    //    foreach (XElement element in el.Elements("results"))
                    //    {
                    //        if (i++ == 0) newMatch.Team1 = element.Attribute("participantname").Value;
                    //        else newMatch.Team2 = element.Attribute("participantname").Value;

                    //    }
                    //    if (!newMatch.Equals(match))
                    //    {
                    //        match = new MatchModel { Team1 = newMatch.Team1, Team2 = newMatch.Team2, Score1 = newMatch.Score1, Score2 = newMatch.Score2, ImageURL = newMatch.ImageURL };
                    //        UpdateUI();
                    //    }
                    //}
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            Thread.Sleep(1000);
        }

        static void SendNotification()
        {
            string message = "";
            string type = events.Last().Type;
                Console.WriteLine(events.Last().Team);
            switch (type)
            {
                case "Regular goal": message = events.Last().PlayerName + " har scoret for " + teams[events.Last().Team]; break;
                case "Own goal": message = events.Last().PlayerName + " fra " + teams[events.Last().Team] + " har scoret et selvmål"; break;
                case "Yellow card": message = events.Last().PlayerName + " fra " + teams[events.Last().Team] + " har fået et gult kort"; break;
                case "Yellow card 2": message = events.Last().PlayerName + " fra " + teams[events.Last().Team] + " har fået to gule kort"; break;
                case "Red card": message = events.Last().PlayerName + " fra " + teams[events.Last().Team] + " har fået et rødt kort"; break;
            }
            RESTTest.Run(message);
        }
    }
}

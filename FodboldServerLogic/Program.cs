using FodboldApp.Model;
using Realms;
using Realms.Sync;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Xml.Linq;


namespace FodboldServerLogic
{
    //uploads played matches to the ROS
    class Program
    {
        static MatchModel newMatch = new MatchModel();
        static MatchModel match = new MatchModel();
        static string fileName = "result.xml";
        static string League = "DBU Pokalen";
        static string ClubName = "Frem";
        static IQueryable<MatchModel> matches;
        static IQueryable<EventModel> events;
        static IQueryable<CommentModel> comments;
        static Realm _realm;

        static void Main1(string[] args)
        {
            Console.WriteLine(RealmConfiguration.DefaultRealmName);
            SetupRealm();

            Thread.Sleep(10000);
        }

        public static async void SetupRealm()
        {
            var user = await User.LoginAsync(Credentials.UsernamePassword("realm-admin", "bachelor", false), new Uri($"http://13.59.205.12:9080"));
            SyncConfiguration config = new SyncConfiguration(user, new Uri($"realm://13.59.205.12:9080/data/matches"));
            Realm.DeleteRealm(config);
            _realm = Realm.GetInstance(config);

            _realm.Write(() =>
            {
                _realm.RemoveAll();
                CheckFile();
            });
        }

        static void CheckFile()
        {
            try
            {
                XDocument doc = XDocument.Load(fileName);

                foreach (XElement el in doc.Root.Elements())
                {
                    if (el.Attribute("league").Value == League)
                    {
                        foreach (XElement XMatchData in el.Elements())
                        {
                            string[] s0 = XMatchData.Attribute("name").Value.Split("-");
                            if (s0[0] == ClubName || s0[1] == ClubName)
                            {
                                string[] teams = XMatchData.Attribute("name").Value.Split("-");
                                MatchModel temp = new MatchModel { Team1 = teams[0], Team2 = teams[1], Id = XMatchData.Attribute("id").Value };
                                int i = 0;
                                foreach (XElement result in XMatchData.Elements("results"))
                                {
                                    if (i++ == 0)
                                    {
                                        temp.Score1 = Int32.Parse(result.Element("result").Attribute("value").Value);
                                    }
                                    else
                                    {
                                        temp.Score2 = Int32.Parse(result.Element("result").Attribute("value").Value);
                                    }
                                }
                                i = 0;
                                foreach (XElement matchEvent in XMatchData.Elements("incidents"))
                                {
                                    if (matchEvent.HasElements)
                                    {
                                        foreach (XElement innerEvent in matchEvent.Elements("incident"))
                                        {
                                            EventModel evtemp = new EventModel { MatchId = temp.Id, PlayerName = innerEvent.Attribute("player").Value, Team = i++, Type = innerEvent.Attribute("type").Value };
                                            _realm.Add(evtemp);
                                            Console.WriteLine(evtemp.Team + " " + evtemp.PlayerName + " " + evtemp.Type);
                                        }
                                    }
                                }
                                _realm.Add(temp);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            Thread.Sleep(500);
        }
    }
}

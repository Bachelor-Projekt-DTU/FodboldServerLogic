using FodboldApp.Model;
using Realms;
using Realms.Sync;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Xml.Linq;

namespace FodboldServerLogic
{
    //uploads future match data to the ROS
    class Fixture
    {
        static string fileName = "fixture.xml";
        static string League = "2. Division";
        static string ClubName = "Frem";
        static Realm _realm;

        static void Msain()
        {
            var user = User.LoginAsync(Credentials.UsernamePassword("realm-admin", "bachelor", false), new Uri($"http://13.59.205.12:9080")).Result;
            SyncConfiguration config = new SyncConfiguration(user, new Uri($"realm://13.59.205.12:9080/data/futureMatches"));
            _realm = Realm.GetInstance(config);
            _realm.Write(() =>
            {
                _realm.RemoveAll();
                XDocument doc = XDocument.Load(fileName);

                foreach (XElement el in doc.Root.Elements())
                {
                    Console.WriteLine(el.Attribute("league").Value);
                    if (el.Attribute("league").Value.Contains(League))
                    {
                        foreach (XElement Fixture in el.Elements("fixture"))
                        {
                            var temp = Fixture.Attribute("name").Value.Split('-');
                            if (temp[0] == ClubName || temp[1] == ClubName)
                            {
                                if (Fixture.Attribute("status").Value == "Not started")
                                {
                                    HeaderMatchModel match = new HeaderMatchModel { Team1 = temp[0], Team2 = temp[1], DateTime = Fixture.Attribute("date").Value, Status = Fixture.Attribute("status").Value, Id = Fixture.Attribute("id").Value };
                                    _realm.Add(match);

                                    Console.WriteLine(match.Team1);
                                    break;
                                }
                            }
                        }
                    }
                }
            }); 

            Console.WriteLine("done");
            Thread.Sleep(10000);
        }

    }
}

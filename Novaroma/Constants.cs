using System;
using System.Collections.Generic;
using System.Linq;
using Novaroma.Model.Search;

namespace Novaroma {

    public static class Constants {
        public const string Novaroma = "Novaroma";
        public const string NetPipeUri = "net.pipe://localhost/";
        public const string NetPipeEndpointName = "NovaromaShellService";
        public const string DefaultMovieSearchPattern = "%movieName% %year%";
        public const string DefaultTvShowEpisodeSearchPattern = "%showName% S%season%E%episode%";
        public const string ImdbTitleUrl = "http://www.imdb.com/title/{0}/";

        public const string EmailRegex =
            @"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?";

        private static readonly Lazy<EnumInfo<Language>[]> _languagesEnumInfo =
            new Lazy<EnumInfo<Language>[]>(() => Helper.GetEnumInfo<Language>().OrderBy(l => l.Item, new CultureLanguageComparer()).ToArray());

        private static readonly Lazy<EnumInfo<OrderFields>[]> _orderFieldsEnumInfo =
            new Lazy<EnumInfo<OrderFields>[]>(() => Helper.GetEnumInfo<OrderFields>().ToArray());

        private static readonly Lazy<EnumInfo<MediaTypes>[]> _mediaTypesEnumInfo =
            new Lazy<EnumInfo<MediaTypes>[]>(() => Helper.GetEnumInfo<MediaTypes>().ToArray());

        private static readonly Lazy<EnumInfo<VideoQuality>[]> _videoQualityEnumInfo =
            new Lazy<EnumInfo<VideoQuality>[]>(() => Helper.GetEnumInfo<VideoQuality>().ToArray());

        public static EnumInfo<Language>[] LanguagesEnumInfo {
            get {
                return _languagesEnumInfo.Value;
            }
        }

        public static EnumInfo<OrderFields>[] OrderFieldsEnumInfo {
            get {
                return _orderFieldsEnumInfo.Value;
            }
        }

        public static EnumInfo<MediaTypes>[] MediaTypesEnumInfo {
            get {
                return _mediaTypesEnumInfo.Value;
            }
        }

        public static EnumInfo<VideoQuality>[] VideoQualityEnumInfo {
            get {
                return _videoQualityEnumInfo.Value;
            }
        }

        public static Dictionary<string, string> Quotes {
            get { return _quotes; }
        }

        #region Codes

        private const string AdvancedInfoProviderDefaultCode =
@"
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Novaroma.Interface.Info;

namespace Novaroma.MyServices {

    public class MyAdvancedInfoProvider : IAdvancedInfoProvider {

        #region IAdvancedInfoProvider Members

        public IEnumerable<string> Genres {
            get { throw new NotImplementedException(); }
        }

        public Task<IEnumerable<IAdvancedInfoSearchResult>> AdvancedSearch(string query, MediaTypes mediaTypes = MediaTypes.All, int? releaseYearStart = null, int? releaseYearEnd = null, 
                                                                           float? ratingMin = null, float? ratingMax = null, int? voteCountMin = null, int? voteCountMax = null, 
                                                                           int? runtimeMin = null, int? runtimeMax = null, IEnumerable<string> genres = null, Languages language = Languages.English) {
            throw new NotImplementedException();
        }

        public Task<IMovieInfo> GetMovie(IAdvancedInfoSearchResult searchResult, Languages language = Languages.English) {
            throw new NotImplementedException();
        }

        public Task<ITvShowInfo> GetTvShow(IAdvancedInfoSearchResult searchResult, Languages language = Languages.English) {
            throw new NotImplementedException();
        }

        #endregion

        #region IInfoProvider Members

        public Task<IEnumerable<IInfoSearchResult>> Search(string query, Languages language = Languages.English) {
            throw new NotImplementedException();
        }

        public Task<IMovieInfo> GetMovie(IInfoSearchResult searchResult, Languages language = Languages.English) {
            throw new NotImplementedException();
        }

        public Task<IMovieInfo> GetMovie(string id, Languages language = Languages.English) {
            throw new NotImplementedException();
        }

        public Task<ITvShowInfo> GetTvShow(IInfoSearchResult searchResult, Languages language = Languages.English) {
            throw new NotImplementedException();
        }

        public Task<ITvShowInfo> GetTvShow(string id, Languages language = Languages.English) {
            throw new NotImplementedException();
        }

        #endregion

        #region INovaromaService Members

        public string ServiceName {
            get { return ""MyAdvancedInfoProvider""; }
        }

        #endregion
    }
}
";

        public const string DownloaderDefaultCode =
@"
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Novaroma.Interface.Download;

namespace Novaroma.MyServices {

    public class MyDownloader : IDownloader {

        #region IDownloader Members

        public Task<IEnumerable<IDownloadSearchResult>> SearchMovie(string name, int? year = null, string imdbId = null, VideoQuality videoQuality = VideoQuality.Any, 
                                                                    string extraKeywords = null, string excludeKeywords = null) {
            throw new NotImplementedException();
        }

        public Task<string> DownloadMovie(string path, string name, int? year = null, string imdbId = null, VideoQuality videoQuality = VideoQuality.Any, 
                                          string extraKeywords = null, string excludeKeywords = null) {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<IDownloadSearchResult>> SearchTvShowEpisode(string name, int season, int episode, string episodeName = null, string imdbId = null, 
                                                                            VideoQuality videoQuality = VideoQuality.Any, string extraKeywords = null, string excludeKeywords = null) {
            throw new NotImplementedException();
        }

        public Task<string> DownloadTvShowEpisode(string path, string name, int season, int episode, string episodeName = null, string imdbId = null, 
                                                  VideoQuality videoQuality = VideoQuality.Any, string extraKeywords = null, string excludeKeywords = null) {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<IDownloadSearchResult>> Search(string query, VideoQuality videoQuality = VideoQuality.Any, string excludeKeywords = null) {
            throw new NotImplementedException();
        }

        public Task<string> Download(string path, IDownloadSearchResult searchResult) {
            throw new NotImplementedException();
        }

        public Task Refresh() {
            throw new NotImplementedException();
        }

        public event EventHandler<DownloadCompletedEventArgs> DownloadCompleted;

        #endregion

        #region INovaromaService Members

        public string ServiceName {
            get { return ""MyDownloader""; }
        }

        #endregion
    }
}
";

        public const string DownloadEventHandlerDefaultCode =
@"
using System;
using Novaroma.Interface.EventHandler;
using Novaroma.Model;

namespace Novaroma.MyServices {

    public class MyDownloadEventHandler : IDownloadEventHandler {

        #region IDownloadEventHandler Members

        public void MovieDownloaded(Movie movie) {
            throw new NotImplementedException();
        }

        public void MovieSubtitleDownloaded(Movie movie) {
            throw new NotImplementedException();
        }

        public void TvShowEpisodeDownloaded(TvShowEpisode episode) {
            throw new NotImplementedException();
        }

        public void TvShowEpisodeSubtitleDownloaded(TvShowEpisode episode) {
            throw new NotImplementedException();
        }

        #endregion

        #region INovaromaService Members

        public string ServiceName {
            get { return ""MyIDownloadEventHandler""; }
        }

        #endregion
    }
}
";

        public const string InfoProviderDefaultCode =
@"
using System;
using Novaroma.Interface.Info;

namespace Novaroma.MyServices {

    public class MyInfoProvider : IInfoProvider {

        #region IInfoProvider Members

        public System.Threading.Tasks.Task<System.Collections.Generic.IEnumerable<IInfoSearchResult>> Search(string query, Languages language = Languages.English) {
            throw new NotImplementedException();
        }

        public System.Threading.Tasks.Task<IMovieInfo> GetMovie(IInfoSearchResult searchResult, Languages language = Languages.English) {
            throw new NotImplementedException();
        }

        public System.Threading.Tasks.Task<IMovieInfo> GetMovie(string id, Languages language = Languages.English) {
            throw new NotImplementedException();
        }

        public System.Threading.Tasks.Task<ITvShowInfo> GetTvShow(IInfoSearchResult searchResult, Languages language = Languages.English) {
            throw new NotImplementedException();
        }

        public System.Threading.Tasks.Task<ITvShowInfo> GetTvShow(string id, Languages language = Languages.English) {
            throw new NotImplementedException();
        }

        #endregion

        #region INovaromaService Members

        public string ServiceName {
            get { return ""MyInfoProvider""; }
        }

        #endregion
    }
}
";

        public const string PluginServiceDefaultCode =
@"
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using Novaroma.Interface;

namespace Novaroma.MyServices {

    public class MyPluginService : IPluginService {

        #region IPluginService Members

        public string DisplayName {
            get { return ""MyPluginService""; }
        }

        public Task Activate() {
            throw new NotImplementedException();
        }

        #endregion

        #region INovaromaService Members

        public string ServiceName {
            get { return ""MyPluginService""; }
        }

        #endregion
    }

}
";

        public const string ShowTrackerDefaultCode =
@"
using System;
using System.Threading.Tasks;
using Novaroma.Interface.Track;

namespace Novaroma.MyServices {

    public class MyShowTracker : IShowTracker {


        #region IShowTracker Members

        public Task<ITvShowUpdate> GetTvShowUpdate(string id, DateTime? lastUpdate = null, Languages language = Languages.English) {
            throw new NotImplementedException();
        }

        public Task<ITvShowUpdate> GetTvShowUpdateByImdbId(string imdbId, DateTime? lastUpdate = null, Languages language = Languages.English) {
            throw new NotImplementedException();
        }

        #endregion

        #region INovaromaService Members

        public string ServiceName {
            get { return ""MyShowTracker""; }
        }

        #endregion
    }
}";

        public const string SubtitleDownloaderDefaultCode =
@"
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Novaroma.Interface.Subtitle;

namespace Novaroma.MyServices {

    public class MySubtitleDownloader : ISubtitleDownloader {

        #region ISubtitleDownloader Members

        public Task<IEnumerable<ISubtitleSearchResult>> SearchForMovie(string name, string videoFilePath, Languages[] languages, string imdbId = null) {
            throw new NotImplementedException();
        }

        public Task<bool> DownloadForMovie(string name, string videoFilePath, Languages[] languages, string imdbId = null) {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ISubtitleSearchResult>> SearchForTvShowEpisode(string name, int season, int episode, string videoFilePath, Languages[] languages, string imdbId = null) {
            throw new NotImplementedException();
        }

        public Task<bool> DownloadForTvShowEpisode(string name, int season, int episode, string videoFilePath, Languages[] languages, string imdbId = null) {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ISubtitleSearchResult>> Search(string query, Languages[] languages, string imdbId = null) {
            throw new NotImplementedException();
        }

        public Task<bool> Download(string videoFilePath, ISubtitleSearchResult searchResult) {
            throw new NotImplementedException();
        }

        public Task<bool> Download(string videoFilePath, Languages[] languages, string imdbId = null) {
            throw new NotImplementedException();
        }

        #endregion

        #region INovaromaService Members

        public string ServiceName {
            get { return ""MySubtitleDownloader""; }
        }

        #endregion
    }
}";

        public const string TorrentMovieProviderDefaultCode =
@"
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Novaroma.Interface.Download.Torrent;
using Novaroma.Interface.Download.Torrent.Provider;

namespace Novaroma.MyServices {

    public class MyTorrentMovieProvider : ITorrentMovieProvider {

        #region ITorrentMovieProvider Members

        public Task<IEnumerable<ITorrentSearchResult>> SearchMovie(string name, int? year = null, string imdbId = null, VideoQuality videoQuality = VideoQuality.Any, 
                                                                   string extraKeywords = null, string excludeKeywords = null, ITorrentDownloader service = null) {
            throw new NotImplementedException();
        }

        #endregion

        #region ITorrentProvider Members

        public Task<IEnumerable<ITorrentSearchResult>> Search(string search, VideoQuality videoQuality = VideoQuality.Any, 
                                                              string excludeKeywords = null, ITorrentDownloader service = null) {
            throw new NotImplementedException();
        }

        #endregion

        #region INovaromaService Members

        public string ServiceName {
            get { return ""MyTorrentMovieProvider""; }
        }

        #endregion
    }
}";

        public const string TorrentTvShowProviderDefaultCode =
@"
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Novaroma.Interface.Download.Torrent;
using Novaroma.Interface.Download.Torrent.Provider;

namespace Novaroma.MyServices {

    public class MyTorrentTvShowProvider : ITorrentTvShowProvider {

        #region ITorrentTvShowProvider Members

        public Task<IEnumerable<ITorrentSearchResult>> SearchTvShowEpisode(string name, int season, int episode, string episodeName, string imdbId = null, 
                                                                           VideoQuality videoQuality = VideoQuality.Any, string extraKeywords = null, 
                                                                           string excludeKeywords = null, ITorrentDownloader service = null) {
            throw new NotImplementedException();
        }

        #endregion

        #region ITorrentProvider Members

        public Task<IEnumerable<ITorrentSearchResult>> Search(string search, VideoQuality videoQuality = VideoQuality.Any, 
                                                              string excludeKeywords = null, ITorrentDownloader service = null) {
            throw new NotImplementedException();
        }

        #endregion

        #region INovaromaService Members

        public string ServiceName {
            get { return ""MyTorrentTvShowProvider""; }
        }

        #endregion
    }
}";

        #endregion

        #region Quotes

        private static readonly Dictionary<string, string> _quotes = new Dictionary<string, string> {
            {"Frankly, my dear, I don't give a damn.", "Rhett Butler (Clark Gable) - Gone with the Wind - 1939"},
            {"I'm going to make him an offer he can't refuse.", "Vito Corleone (Marlon Brando) - The Godfather - 1972"}, {
                "You don't understand! I coulda had class. I coulda been a contender. I could've been somebody, instead of a bum, which is what I am.",
                "Terry Malloy (Marlon Brando) - On the Waterfront - 1954"
            }, {
                "Toto, I've got a feeling we're not in Kansas anymore.",
                "Dorothy Gale (Judy Garland) - The Wizard of Oz - 1939"
            },
            {"Here's looking at you, kid.", "Rick Blaine (Humphrey Bogart) - Casablanca - 1942"},
            {"Go ahead, make my day.", "Harry Callahan (Clint Eastwood) - Sudden Impact - 1983"}, {
                "All right, Mr. DeMille, I'm ready for my close-up.",
                "Norma Desmond (Gloria Swanson) - Sunset Boulevard - 1950"
            },
            {"May the Force be with you.", "Han Solo (Harrison Ford) - Star Wars - 1977"}, {
                "Fasten your seatbelts. It's going to be a bumpy night.",
                "Margo Channing (Bette Davis) - All About Eve - 1950"
            },
            {"You talkin' to me?", "Travis Bickle (Robert De Niro) - Taxi Driver - 1976"},
            {"What we've got here is failure to communicate.", "Captain (Strother Martin) - Cool Hand Luke - 1967"}, {
                "I love the smell of napalm in the morning.",
                "Lt. Col. Bill Kilgore (Robert Duvall) - Apocalypse Now - 1979"
            }, {
                "Love means never having to say you're sorry.",
                "Jennifer Cavilleri Barrett (Ali MacGraw) - Love Story - 1970"
            },
            {"The stuff that dreams are made of.", "Sam Spade (Humphrey Bogart) - The Maltese Falcon - 1941"},
            {"E.T. phone home.", "E.T. (Pat Welsh) - E.T. the Extra-Terrestrial - 1982"},
            {"They call me Mister Tibbs!", "Virgil Tibbs (Sidney Poitier) - In the Heat of the Night - 1967"},
            {"Rosebud.", "Charles Foster Kane (Orson Welles) - Citizen Kane - 1941"},
            {"Made it, Ma! Top of the world!", "Arthur 'Cody' Jarrett (James Cagney) - White Heat - 1949"}, {
                "I'm as mad as hell, and I'm not going to take this anymore!",
                "Howard Beale (Peter Finch) - Network - 1976"
            }, {
                "Louis, I think this is the beginning of a beautiful friendship.",
                "Rick Blaine (Humphrey Bogart) - Casablanca - 1942"
            }, {
                "A census taker once tried to test me. I ate his liver with some fava beans and a nice Chianti.",
                "Hannibal Lecter (Anthony Hopkins) - The Silence of the Lambs - 1991"
            },
            {"Bond. James Bond.", "James Bond (Sean Connery) - Dr. No - 1962"},
            {"There's no place like home.", "Dorothy Gale (Judy Garland) - The Wizard of Oz - 1939"},
            {"I am big! It's the pictures that got small.", "Norma Desmond (Gloria Swanson) - Sunset Boulevard - 1950"},
            {"Show me the money!", "Rod Tidwell (Cuba Gooding, Jr.) - Jerry Maguire - 1996"},
            {"Why don't you come up sometime and see me?", "Lady Lou (Mae West) - She Done Him Wrong - 1933"},
            {"I'm walking here! I'm walking here!", "'Ratso' Rizzo (Dustin Hoffman) - Midnight Cowboy - 1969"},
            {"Play it, Sam. Play 'As Time Goes By.'", "Ilsa Lund (Ingrid Bergman) - Casablanca - 1942"},
            {"You can't handle the truth!", "Col. Nathan R. Jessup (Jack Nicholson) - A Few Good Men - 1992"},
            {"I want to be alone.", "Grusinskaya (Greta Garbo) - Grand Hotel - 1932"},
            {"After all, tomorrow is another day!", "Scarlett O'Hara (Vivien Leigh) - Gone with the Wind - 1939"},
            {"Round up the usual suspects.", "Capt. Louis Renault (Claude Rains) - Casablanca - 1942"},
            {"I'll have what she's having.", "Customer (Estelle Reiner) - When Harry Met Sally... - 1989"}, {
                "You know how to whistle, don't you, Steve? You just put your lips together and blow.",
                "Marie 'Slim' Browning (Lauren Bacall) - To Have and Have Not - 1944"
            },
            {"You're gonna need a bigger boat.", "Martin Brody (Roy Scheider) - Jaws - 1975"}, {
                "Badges? We ain't got no badges! We don't need no badges! I don't have to show you any stinking badges!",
                "'Gold Hat' (Alfonso Bedoya) - The Treasure of the Sierra Madre - 1948"
            },
            {"I'll be back.", "The Terminator (Arnold Schwarzenegger) - The Terminator - 1984"}, {
                "Today, I consider myself the luckiest man on the face of the Earth.",
                "Lou Gehrig (Gary Cooper) - The Pride of the Yankees - 1942"
            },
            {"If you build it, he will come.", "Shoeless Joe Jackson (Ray Liotta (voice)) - Field of Dreams - 1989"}, {
                "Mama always said life was like a box of chocolates. You never know what you're gonna get.",
                "Forrest Gump (Tom Hanks) - Forrest Gump - 1994"
            },
            {"We rob banks.", "Clyde Barrow (Warren Beatty) - Bonnie and Clyde - 1967"},
            {"Plastics.", "Mr. Maguire (Walter Brooke) - The Graduate - 1967"},
            {"We'll always have Paris.", "Rick Blaine (Humphrey Bogart) - Casablanca - 1942"},
            {"I see dead people.", "Cole Sear (Haley Joel Osment) - The Sixth Sense - 1999"},
            {"Stella! Hey, Stella!", "Stanley Kowalski (Marlon Brando) - A Streetcar Named Desire - 1951"}, {
                "Oh, Jerry, don't let's ask for the moon. We have the stars.",
                "Charlotte Vale (Bette Davis) - Now, Voyager - 1942"
            },
            {"Shane. Shane. Come back!", "Joey Starrett (Brandon De Wilde) - Shane - 1953"},
            {"Well, nobody's perfect.", "Osgood Fielding III (Joe E. Brown) - Some Like It Hot - 1959"},
            {"It's alive! It's alive!", "Henry Frankenstein (Colin Clive) - Frankenstein - 1931"},
            {"Houston, we have a problem.", "Jim Lovell (Tom Hanks) - Apollo 13 - 1995"}, {
                "You've got to ask yourself one question: 'Do I feel lucky?' Well, do ya, punk?",
                "Harry Callahan (Clint Eastwood) - Dirty Harry - 1971"
            },
            {"You had me at 'hello.'", "Dorothy Boyd (Renée Zellweger) - Jerry Maguire - 1996"}, {
                "One morning I shot an elephant in my pajamas. How he got in my pajamas, I don't know.",
                "Capt. Geoffrey T. Spaulding (Groucho Marx) - Animal Crackers - 1930"
            },
            {"There's no crying in baseball!", "Jimmy Dugan (Tom Hanks) - A League of Their Own - 1992"},
            {"La-dee-da, la-dee-da.", "Annie Hall (Diane Keaton) - Annie Hall - 1977"},
            {"A boy's best friend is his mother.", "Norman Bates (Anthony Perkins) - Psycho - 1960"},
            {"Greed, for lack of a better word, is good.", "Gordon Gekko (Michael Douglas) - Wall Street - 1987"}, {
                "Keep your friends close, but your enemies closer.",
                "Michael Corleone (Al Pacino) - The Godfather Part II - 1974"
            }, {
                "As God is my witness, I'll never be hungry again.",
                "Scarlett O'Hara (Vivien Leigh) - Gone with the Wind - 1939"
            }, {
                "Well, here's another nice mess you've gotten me into!",
                "Oliver (Oliver Hardy) - Sons of the Desert - 1933"
            },
            {"Say 'hello' to my little friend!", "Tony Montana (Al Pacino) - Scarface - 1983"},
            {"What a dump.", "Rosa Moline (Bette Davis) - Beyond the Forest - 1949"}, {
                "Mrs. Robinson, you're trying to seduce me. Aren't you?",
                "Benjamin Braddock (Dustin Hoffman) - The Graduate - 1967"
            }, {
                "Gentlemen, you can't fight in here! This is the War Room!",
                "President Merkin Muffley (Peter Sellers) - Dr. Strangelove or: How I Learned to Stop Worrying and Love the Bomb - 1964"
            }, {
                "Elementary, my dear Watson.",
                "Sherlock Holmes (Basil Rathbone) - The Adventures of Sherlock Holmes - 1939"
            }, {
                "Get your stinking paws off me, you damned dirty ape.",
                "George Taylor (Charlton Heston) - Planet of the Apes - 1968"
            }, {
                "Of all the gin joints in all the towns in all the world, she walks into mine.",
                "Rick Blaine (Humphrey Bogart) - Casablanca - 1942"
            },
            {"Here's Johnny!", "Jack Torrance (Jack Nicholson) - The Shining - 1980"},
            {"They're here!", "Carol Anne Freeling (Heather O'Rourke) - Poltergeist - 1982"},
            {"Is it safe?", "Dr. Christian Szell (Laurence Olivier) - Marathon Man - 1976"}, {
                "Wait a minute, wait a minute. You ain't heard nothin' yet!",
                "Jakie Rabinowitz/Jack Robin (Al Jolson) - The Jazz Singer - 1927"
            },
            {"No wire hangers, ever!", "Joan Crawford (Faye Dunaway) - Mommie Dearest - 1981"}, {
                "Mother of mercy, is this the end of Rico?",
                "Cesare Enrico 'Rico' Bandello (Edward G. Robinson) - Little Caesar - 1930"
            },
            {"Forget it, Jake, it's Chinatown.", "Lawrence Walsh (Joe Mantell) - Chinatown - 1974"}, {
                "I have always depended on the kindness of strangers.",
                "Blanche DuBois (Vivien Leigh) - A Streetcar Named Desire - 1951"
            },
            {"Hasta la vista, baby.", "The Terminator (Arnold Schwarzenegger) - Terminator 2: Judgment Day - 1991"},
            {"Soylent Green is people!", "Det. Robert Thorn (Charlton Heston) - Soylent Green - 1973"},
            {"Open the pod bay doors please, HAL.", "Dave Bowman (Keir Dullea) - 2001: A Space Odyssey - 1968"}, {
                "Striker: Surely you can't be serious. Rumack: I am serious...and don't call me Shirley.",
                "Ted Striker and Dr. Rumack (Robert Hays and Leslie Nielsen) - Airplane! - 1980"
            },
            {"Yo, Adrian!", "Rocky Balboa (Sylvester Stallone) - Rocky - 1976"},
            {"Hello, gorgeous.", "Fanny Brice (Barbra Streisand) - Funny Girl - 1968"},
            {"Toga! Toga!", "John 'Bluto' Blutarsky (John Belushi) - National Lampoon's Animal House - 1978"}, {
                "Listen to them. Children of the night. What music they make.",
                "Count Dracula (Bela Lugosi) - Dracula - 1931"
            }, {
                "Oh, no, it wasn't the airplanes. It was Beauty killed the Beast.",
                "Carl Denham (Robert Armstrong) - King Kong - 1933"
            },
            {"My precious.", "Gollum (Andy Serkis) - The Lord of the Rings: The Two Towers - 2002"},
            {"Attica! Attica!", "Sonny Wortzik (Al Pacino) - Dog Day Afternoon - 1975"}, {
                "Sawyer, you're going out a youngster, but you've got to come back a star!",
                "Julian Marsh (Warner Baxter) - 42nd Street - 1933"
            }, {
                "Listen to me, mister. You're my knight in shining armor. Don't you forget it. You're going to get back on that horse, and I'm going to be right behind you, holding on tight, and away we're gonna go, go, go!",
                "Ethel Thayer (Katharine Hepburn) - On Golden Pond - 1981"
            }, {
                "Tell 'em to go out there with all they got and win just one for the Gipper.",
                "Knute Rockne (Pat O'Brien) - Knute Rockne, All American - 1940"
            },
            {"A martini. Shaken, not stirred.", "James Bond (Sean Connery) - Goldfinger - 1964"},
            {"Who's on first?", "Dexter (Bud Abbott) - The Naughty Nineties - 1945"}, {
                "Cinderella story. Outta nowhere. A former greenskeeper, now, about to become the Masters champion. It looks like a mirac...It's in the hole! It's in the hole! It's in the hole!",
                "Carl Spackler (Bill Murray) - Caddyshack - 1980"
            }, {
                "Life is a banquet, and most poor suckers are starving to death!",
                "Mame Dennis (Rosalind Russell) - Auntie Mame - 1958"
            }, {
                "I feel the need—the need for speed!",
                "Lt. Pete 'Maverick' Mitchell and Lt. Nick 'Goose' Bradshaw (Tom Cruise and Anthony Edwards) - Top Gun - 1986"
            }, {
                "Carpe diem. Seize the day, boys. Make your lives extraordinary.",
                "John Keating (Robin Williams) - Dead Poets Society - 1989"
            },
            {"Snap out of it!", "Loretta Castorini (Cher) - Moonstruck - 1987"}, {
                "My mother thanks you. My father thanks you. My sister thanks you. And I thank you.",
                "George M. Cohan (James Cagney) - Yankee Doodle Dandy - 1942"
            },
            {"Nobody puts Baby in a corner.", "Johnny Castle (Patrick Swayze) - Dirty Dancing - 1987"}, {
                "I'll get you, my pretty, and your little dog too!",
                "Wicked Witch of the West (Margaret Hamilton) - The Wizard of Oz - 1939"
            },
            {"I'm the king of the world!", "Jack Dawson (Leonardo DiCaprio) - Titanic - 1997"},
        };

        #endregion
    }
}

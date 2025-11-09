using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using ProtoBuf;
using System;
using System.Net;
using static AtacFeed.TransitRealtime;
using Log = Serilog.Log;

namespace AtacFeed
{
    public class BaseFeedManager
    {
        public static readonly DateTime t0 = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        protected FeedMessage LastReadFeed { get; set; }
        public FeedMessage LastValidFeed { get; set; }
        public FeedMessage PrevValidFeed { get; set; }
        public DateTime? LastDataFeed;
        public DateTime? FirstDataFeed;
        public int LastValidationResultCode { get; set; } = -20;
        public int CodeFeed { get; set; } = -1;

        public void LeggiFeed(string url)
        {
            try
            {
                CodeFeed = 1;

                var request = WebRequest.Create(url);
                request.Timeout = 10000;

                using (var response = request.GetResponse())
                using (var stream = response.GetResponseStream())
                {
                    if (stream == null)
                    {
                        LastReadFeed = null;
                        CodeFeed = 0;
                        Log.Error("LeggiFeed {Url} - Response stream is null", url);
                        throw new InvalidOperationException("Response stream is null");
                    }

                    // Deserialize directly from the response stream and assign
                    LastReadFeed = Serializer.Deserialize<FeedMessage>(stream);
                }
            }
            catch (Exception exc)
            {
                // Maintain prior behaviour: log, set state and rethrow preserving stack trace
                LastReadFeed = null;
                CodeFeed = 0;
                Log.Error(exc, "LeggiFeed {Url} - {Message}", url, exc.Message);
                throw;
            }
        }

        public int ValidaFeed()
        {
            int isValid = 0;

            try
            {
                if (LastReadFeed == null)
                {
                    Log.Error("[{Time}] - Feed Scartato perchè NON LETTO", DateTime.Now.ToString("HH:mm:ss"));
                    isValid = -1;
                }
                else if (LastReadFeed.Entities == null || LastReadFeed.Entities.Count == 0)
                {
                    Log.Error("[{Time}] - Feed Scartato perchè VUOTO", DateTime.Now.ToString("HH:mm:ss"));
                    isValid = -2;
                }
                else
                {
                    // Compare timestamps as integers first (cheaper than DateTime conversions)
                    ulong newTs = LastReadFeed.Header != null ? LastReadFeed.Header.Timestamp : 0;
                    ulong prevTs = LastValidFeed != null && LastValidFeed.Header != null ? LastValidFeed.Header.Timestamp : 0;

                    if (prevTs >= newTs)
                    {
                        Log.Error("[{Time}] - Feed scartato in quanto ha il timestamp SUPERATO", DateTime.Now.ToString("HH:mm:ss"));
                        isValid = -3;
                    }
                    else
                    {
                        // Valid feed: update references and timestamps once
                        PrevValidFeed = LastValidFeed;
                        LastValidFeed = LastReadFeed;

                        DateTime feedDate = t0.AddSeconds(newTs).ToLocalTime();
                        LastDataFeed = feedDate;
                        if (!FirstDataFeed.HasValue)
                        {
                            FirstDataFeed = feedDate;
                        }

                        isValid = 0;
                    }
                }
            }
            catch (Exception exc)
            {
                Log.Error(exc, "Errore Generico in ValidaFeed");
                isValid = -10;
            }

            LastValidationResultCode = isValid;
            return LastValidationResultCode;
        }

        public int LeggiFeedValido(string url)
        {
            LeggiFeed(url);
            CodeFeed = ValidaFeed();
            return CodeFeed;
        }

        public virtual void Reset()
        {
            LastReadFeed = null;
            LastValidFeed = null;
            LastDataFeed = null;
            FirstDataFeed = null;
            LastValidationResultCode = -20;
            CodeFeed = -1;
        }
    }
}
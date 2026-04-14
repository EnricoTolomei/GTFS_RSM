using ProtoBuf;
using System;
using System.Net;
using static AtacFeed.TransitRealtime;
using Log = Serilog.Log;

namespace AtacFeed
{
    public class BaseFeedManager
    {
        private const int DefaultTimeoutMs = 10000;

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
            if (string.IsNullOrWhiteSpace(url))
            {
                Log.Error("LeggiFeed chiamato con url vuoto");
                LastReadFeed = null;
                throw new ArgumentException("url non valido", nameof(url));
            }

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Timeout = DefaultTimeoutMs;
                request.ReadWriteTimeout = DefaultTimeoutMs;
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                using (var response = request.GetResponse())
                using (var stream = response.GetResponseStream())
                {
                    if (stream == null)
                    {
                        LastReadFeed = null;
                        Log.Error("LeggiFeed {Url} - Response stream is null", url);
                        throw new InvalidOperationException("Response stream is null");
                    }

                    // Deserialize directly from the response stream
                    LastReadFeed = Serializer.Deserialize<FeedMessage>(stream);
                }
            }
            catch (WebException wex)
            {
                LastReadFeed = null;
                if (wex.Response is HttpWebResponse resp)
                {
                    Log.Error(wex, "LeggiFeed {Url} - WebException Status={Status} Code={StatusCode} Uri={Uri}", url, wex.Status, resp.StatusCode, resp.ResponseUri);
                }
                else
                {
                    Log.Error(wex, "LeggiFeed {Url} - WebException Status={Status}", url, wex.Status);
                }
                throw wex;
            }
            catch (Exception exc)
            {
                LastReadFeed = null;
                Log.Error(exc, "LeggiFeed {Url} - {Message}", url, exc.Message);
                throw exc;
            }
        }

        public int ValidaFeed()
        {
            int isValid = 0;

            try
            {
                string now = DateTime.Now.ToString("HH:mm:ss");

                if (LastReadFeed == null)
                {
                    Log.Error("[{Time}] - Feed Scartato perchè NON LETTO", now);
                    isValid = -1;
                }
                else if (LastReadFeed.Entities == null || LastReadFeed.Entities.Count == 0)
                {
                    Log.Error("[{Time}] - Feed Scartato perchè VUOTO", now);
                    isValid = -2;
                }
                else
                {
                    // Leggi timestamp come long (compatibile con tipi protobuf comuni)
                    long newTs = 0;
                    long prevTs = 0;

                    if (LastReadFeed.Header != null)
                        newTs = (long)LastReadFeed.Header.Timestamp;

                    if (LastValidFeed != null && LastValidFeed.Header != null)
                        prevTs = (long)LastValidFeed.Header.Timestamp;

                    if (prevTs >= newTs)
                    {
                        Log.Error("[{Time}] - Feed scartato in quanto ha il timestamp SUPERATO {Time} >= {Time}", now, prevTs ,newTs);
                        isValid = -3;
                    }
                    else
                    {
                        PrevValidFeed = LastValidFeed;
                        LastValidFeed = LastReadFeed;

                        // Calcola LastDataFeed solo quando feed valido
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
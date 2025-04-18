﻿using ProtoBuf;
using Serilog;
using System;
using System.Net;
using static AtacFeed.TransitRealtime;

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
                WebRequest request = WebRequest.Create(url);
                request.Timeout = 10000;
                WebResponse response = request.GetResponse();
                var stream = response.GetResponseStream();
                LastReadFeed = Serializer.Deserialize<FeedMessage>(stream);
            }
            catch (Exception exc)
            {
                LastReadFeed = null;
                Log.Error(exc, "LeggiFeed {@url} - {@message}", url, exc.Message);
                CodeFeed = 0;
                throw (exc);
            }
        }
        public int ValidaFeed()
        {
            int isValid;
            try
            {
                if (LastReadFeed == null)
                {
                    Log.Error($"[{DateTime.Now:HH:mm:ss}] - Feed Scartato perchè NON LETTO");
                    isValid = -1;
                }
                else if (LastReadFeed.Entities.Count == 0)
                {
                    Log.Error($"[{DateTime.Now:HH:mm:ss}] - Feed Scartato perchè VUOTO");
                    isValid = -2;
                }
                else if ((LastValidFeed?.Header.Timestamp ?? 0) >= (LastReadFeed?.Header.Timestamp ?? 0))
                {
                    Log.Error($"[{DateTime.Now:HH:mm:ss}] - Feed scartato in quanto ha il timestamp SUPERATO");
                    isValid = -3;
                }
                else
                {
                    isValid = 0;
                    DateTime lastDate = t0.AddSeconds(LastValidFeed?.Header.Timestamp ?? 0).ToLocalTime();
                    DateTime feedDate = t0.AddSeconds(LastReadFeed?.Header.Timestamp ?? 0).ToLocalTime();
                    PrevValidFeed = LastValidFeed;
                    LastValidFeed = LastReadFeed;
                    LastDataFeed = feedDate;
                    if (!FirstDataFeed.HasValue)
                    {
                        FirstDataFeed = feedDate;
                    }
                }
            }
            catch (Exception exc)
            {
                Log.Error(exc, "Errore Generico");
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
        }
    }
}
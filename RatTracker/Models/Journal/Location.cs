﻿using Newtonsoft.Json;

namespace RatTracker.Models.Journal
{
  public class Location : JournalEntryBase
  {
    [JsonProperty("StarPos")]
    public double[] Coordinates { get; set; }

    [JsonProperty("StarSystem")]
    public string SystemName { get; set; }

    [JsonProperty("Population")]
    public long Population { get; set; }
  }
}
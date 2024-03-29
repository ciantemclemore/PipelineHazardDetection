﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Online_Final_Computer_Architecture
{
    public class HazardConfirmation
    {
        public string Name { get; set; }

        public string Instruction { get; set; }

        public List<string> Registers { get; set; } = new List<string>();

        public int StallCount { get; set; } = 0;

        public string Message { get; set; }

        public bool IsHazard { get; set; }
    }
}

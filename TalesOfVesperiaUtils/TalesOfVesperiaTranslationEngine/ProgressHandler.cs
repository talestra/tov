using CSharpUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TalesOfVesperiaTranslationEngine
{
    sealed public class ProgressHandler
    {
        internal class ProgressLevel
        {
            internal ProgressHandler ProgressHandler;
            internal ProgressLevel Next;

            public string Description = "";
            public int Level;
            public long Current;
            public long Total;

            public double ProcessedLevelProgress
            {
                get
                {
                    double Increment = (Total != 0) ? ((double)1 / (double)Total) : 1;
                    double Progress = Increment * Current;
                    if (Next != null) Progress += Next.ProcessedLevelProgress * Increment;
                    //Console.WriteLine(Progress);
                    return MathUtils.Clamp(Progress, 0, 1);
                }
            }
        }

        List<ProgressLevel> ProgressLevels = new List<ProgressLevel>();

        public event Action OnProgressUpdated;

        private ProgressLevel GetLevel(int Level)
        {
            while (ProgressLevels.Count <= Level) 
            {
                var NewLevel = new ProgressLevel() { ProgressHandler = this, Level = ProgressLevels.Count, Current = 0, Total = 0 };
                if (ProgressLevels.Count > 0)
                {
                    ProgressLevels.Last().Next = NewLevel;
                }
                ProgressLevels.Add(NewLevel);
            }
            return ProgressLevels[Level];
        }

        public string GetLevelDescription(int Level)
        {
            return GetLevel(Level).Description;
        }

        public string GetLevelDescriptionChain(int Level)
        {
            string Description = GetLevelDescription(Level);
            if (Level < ProgressLevels.Count - 1)
            {
                Description += " / " + GetLevelDescriptionChain(Level + 1);
            }
            return Description;
        }

        public double GetProcessedLevelProgress(int Level)
        {
            return GetLevel(Level).ProcessedLevelProgress;
        }

        private int CurrentProgressLevel = -1;

        private void SetLevelTotal(string Description, long Total)
        {
            var Level = this.GetLevel(CurrentProgressLevel);
            Level.Current = 0;
            Level.Total = Total;
            Level.Description = Description;
        }

        public void IncrementLevelProgress()
        {
            lock (this)
            {
                this.GetLevel(CurrentProgressLevel).Current++;
                this.ProgressUpdated();
            }
        }

        private void ProgressUpdated()
        {
            if (OnProgressUpdated != null) OnProgressUpdated();
        }

        public void AddProgressLevel(string Description, int Total, Action Action)
        {
            this.CurrentProgressLevel++;
            this.SetLevelTotal(Description, Total);
            this.ProgressUpdated();
            try
            {
                Action();
            }
            finally
            {
                var Level = this.GetLevel(CurrentProgressLevel);
                if (Level.Current < Level.Total)
                {
                    Level.Current = 1;
                    Level.Total = 1;
                    ProgressUpdated();
                }
                this.SetLevelTotal("", 0);
                this.CurrentProgressLevel--;
            }
        }

        public void ExecuteActionsWithProgressTracking(string Description, params Action[] Actions)
        {
            ExecuteActionsWithProgressTracking(Description, (IEnumerable<Action>)Actions);
        }

        public void ExecuteActionsWithProgressTracking(string Description, IEnumerable<Action> Actions)
        {
            AddProgressLevel(Description, Actions.Count(), () =>
            {
                foreach (var Action in Actions)
                {
                    Action();
                    this.IncrementLevelProgress();
                }
            });
        }
    }
}

using static RL.Classifiers.ConfusionMatrixStatus;

namespace RL.Classifiers
{
    public struct ClassifierResultingContainer
    {
        public int TPCount { get; set; }
        public int TNCount { get; set; }
        public int FPCount { get; set; }
        public int FNCount { get; set; }
        public int TotalEntryCount { get; set; }

        public readonly int GetCount(ConfusionMatrixStatus status)
        {
            return status switch
            {
                TruePositive => TPCount,
                TrueNegative => TNCount,
                FalsePositive => FPCount,
                FalseNegative => FNCount,
                _ => 0,
            };
        }

        public void IncrementCount(ConfusionMatrixStatus status)
        {
            switch (status)
            {
                case TruePositive:
                    TPCount++;
                    break;
                case TrueNegative:
                    TNCount++;
                    break;
                case FalsePositive:
                    FPCount++;
                    break;
                case FalseNegative:
                    FNCount++;
                    break;
            }
        }
        
        public void DecrementCount(ConfusionMatrixStatus status)
        {
            switch (status)
            {
                case TruePositive:
                    TPCount--;
                    break;
                case TrueNegative:
                    TNCount--;
                    break;
                case FalsePositive:
                    FPCount--;
                    break;
                case FalseNegative:
                    FNCount--;
                    break;
            }
        }
        
        public float CalculatePrecision()
        {
            // Precision = TP / (TP + FP)
            return (TPCount + FPCount) == 0 ? 0 : (float)TPCount / (TPCount + FPCount);
        }

        public float CalculateRecall()
        {
            // Recall = TP / (TP + FN)
            return (TPCount + FNCount) == 0 ? 0 : (float)TPCount / (TPCount + FNCount);
        }

        public float CalculateFScore()
        {
            float precision = CalculatePrecision();
            float recall = CalculateRecall();
            // F-Score = 2 * (Precision * Recall) / (Precision + Recall)
            return (precision + recall) == 0 ? 0 : 2 * (precision * recall) / (precision + recall);
        }
    }
}
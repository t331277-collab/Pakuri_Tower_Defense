using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Pakuri.NewCore.Units.Models;

namespace Pakuri.NewCore.Run
{
    public sealed class StageManager
    {
        private readonly List<UnitBaseModel> fieldUnits = new List<UnitBaseModel>();
        private readonly IReadOnlyList<UnitBaseModel> readOnlyFieldUnits;
        private int gold;
        private int darkTrace;

        public StageManager(RunSessionModel session, int initialGold, int initialDarkTrace)
        {
            Session = session ?? throw new ArgumentNullException(nameof(session));
            if (initialGold < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(initialGold));
            }

            if (initialDarkTrace < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(initialDarkTrace));
            }

            gold = initialGold;
            darkTrace = initialDarkTrace;
            readOnlyFieldUnits = new ReadOnlyCollection<UnitBaseModel>(fieldUnits);
        }

        public RunSessionModel Session { get; }

        public int Gold => gold;

        public int DarkTrace => darkTrace;

        public IReadOnlyList<UnitBaseModel> FieldUnits => readOnlyFieldUnits;

        public IReadOnlyList<UnitBaseModel> LivingFieldUnits
        {
            get
            {
                List<UnitBaseModel> living = new List<UnitBaseModel>();
                for (int index = 0; index < fieldUnits.Count; index++)
                {
                    if (fieldUnits[index].IsAlive)
                    {
                        living.Add(fieldUnits[index]);
                    }
                }

                return living.AsReadOnly();
            }
        }

        public void AddGold(int amount)
        {
            gold = AddCurrency(gold, amount, nameof(amount));
        }

        public bool CanSpendGold(int amount)
        {
            return CanSpend(gold, amount);
        }

        public bool SpendGold(int amount)
        {
            if (!CanSpendGold(amount))
            {
                return false;
            }

            gold -= amount;
            return true;
        }

        public void AddDarkTrace(int amount)
        {
            darkTrace = AddCurrency(darkTrace, amount, nameof(amount));
        }

        public bool CanSpendDarkTrace(int amount)
        {
            return CanSpend(darkTrace, amount);
        }

        public bool SpendDarkTrace(int amount)
        {
            if (!CanSpendDarkTrace(amount))
            {
                return false;
            }

            darkTrace -= amount;
            return true;
        }

        public bool TryRegisterFieldUnit(UnitBaseModel unit)
        {
            if (unit == null || fieldUnits.Contains(unit))
            {
                return false;
            }

            fieldUnits.Add(unit);
            return true;
        }

        public bool TryUnregisterFieldUnit(UnitBaseModel unit)
        {
            return unit != null && fieldUnits.Remove(unit);
        }

        public void ClearFieldUnits()
        {
            fieldUnits.Clear();
        }

        private static int AddCurrency(int current, int amount, string parameterName)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }

            return checked(current + amount);
        }

        private static bool CanSpend(int current, int amount)
        {
            return amount >= 0 && current >= amount;
        }
    }
}

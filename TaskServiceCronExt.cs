﻿using System;
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.Win32.TaskScheduler
{
	public abstract partial class Trigger
	{
		/// <summary>
		/// Creates a trigger using a cron string.
		/// </summary>
		/// <param name="cronString">String using cron defined syntax for specifying a time interval.</param>
		/// <returns>Array of <see cref="Trigger" /> representing the specified cron string.</returns>
		/// <exception cref="System.NotImplementedException">Unsupported cron string.</exception>
		/// <remarks>
		///   <para>NOTE: This method is a work in progress and does not support all combinations of cron strings. Please test extensively before use.</para>
		///   <para>Currently the cronString only supports numbers and not any of the weekday or month string. Please use numeric equivalent.</para>
		/// </remarks>
		public static Trigger[] CreateTriggers(string cronString)
		{
			CronExpression cron = new CronExpression();
			cron.Parse(cronString);

			// TODO: Figure out all the permutations of expression and convert to Trigger(s)
			/* Time (fields 1-4 have single number and dow = *)
			 * Time repeating
			 * Daily
			 * Weekly
			 * Monthly
			 * Monthly DOW
			 */

			List<Trigger> ret = new List<Trigger>();

			// MonthlyDOWTrigger
			if (!cron.DOW.IsEvery)
			{
				// Determine DOW
				DaysOfTheWeek dow = 0;
				if (cron.DOW.vals.Length == 0)
					dow = DaysOfTheWeek.AllDays;
				else if (cron.DOW.range)
					for (int i = cron.DOW.vals[0]; i <= cron.DOW.vals[1]; i += cron.DOW.step)
						dow |= (DaysOfTheWeek)(1 << (i - 1));
				else
					for (int i = 0; i < cron.DOW.vals.Length; i++)
						dow |= (DaysOfTheWeek)(1 << (cron.DOW.vals[i] - 1));

				// Determine months
				MonthsOfTheYear moy = 0;
				if ((cron.Months.vals.Length == 0 || (cron.Months.vals.Length == 1 && cron.Months.vals[0] == 1)) && cron.Months.IsEvery)
					moy = MonthsOfTheYear.AllMonths;
				else if (cron.Months.range)
					for (int i = cron.Months.vals[0]; i <= cron.Months.vals[1]; i += cron.Months.step)
						moy |= (MonthsOfTheYear)(1 << (i - 1));
				else
					for (int i = 0; i < cron.Months.vals.Length; i++)
						moy |= (MonthsOfTheYear)(1 << (cron.Months.vals[i] - 1));

				Trigger tr = new MonthlyDOWTrigger(dow, moy, WhichWeek.AllWeeks);
				ret.AddRange(ProcessCronTimes(cron, tr));
			}
			// MonthlyTrigger
			else if (cron.Days.vals.Length > 0)
			{
				// Determine DOW
				List<int> days = new List<int>();
				if (cron.Days.range)
					for (int i = cron.Days.vals[0]; i <= cron.Days.vals[1]; i += cron.Days.step)
						days.Add(i);
				else
					for (int i = 0; i < cron.Days.vals.Length; i++)
						days.Add(cron.Days.vals[i]);

				// Determine months
				MonthsOfTheYear moy = 0;
				if ((cron.Months.vals.Length == 0 || (cron.Months.vals.Length == 1 && cron.Months.vals[0] == 1)) && cron.Months.IsEvery)
					moy = MonthsOfTheYear.AllMonths;
				else if (cron.Months.range)
					for (int i = cron.Months.vals[0]; i <= cron.Months.vals[1]; i += cron.Months.step)
						moy |= (MonthsOfTheYear)(1 << (i - 1));
				else
					for (int i = 0; i < cron.Months.vals.Length; i++)
						moy |= (MonthsOfTheYear)(1 << (cron.Months.vals[i] - 1));

				Trigger tr = new MonthlyTrigger(1, moy) { DaysOfMonth = days.ToArray() };
				ret.AddRange(ProcessCronTimes(cron, tr));
			}
			// DailyTrigger
			else if (cron.Months.IsEvery && cron.DOW.IsEvery && cron.Days.repeating)
			{
				Trigger tr = new DailyTrigger((short)cron.Days.step);
				ret.AddRange(ProcessCronTimes(cron, tr));
			}
			else
			{
				throw new NotImplementedException();
			}

			return ret.ToArray();
		}

		private static IEnumerable<Trigger> ProcessCronTimes(CronExpression cron, Trigger baseTrigger)
		{
			List<Trigger> ret = new List<Trigger>();
			// A single time
			if (cron.Minutes.vals.Length == 1 && cron.Hours.vals.Length == 1)
			{
				baseTrigger.StartBoundary = baseTrigger.StartBoundary.Date + new TimeSpan(cron.Hours.vals[0], cron.Minutes.vals[0], 0);
				ret.Add(baseTrigger);
			}
			// Multiple, non-repeating, hours and/or minutes
			else if (cron.Minutes.vals.Length > 1 && !cron.Minutes.range && cron.Hours.vals.Length > 1 && !cron.Hours.range)
			{
				for (int h = 0; h < cron.Hours.vals.Length; h++)
				{
					for (int m = 0; m < cron.Minutes.vals.Length; m++)
					{
						Trigger newTr = (Trigger)baseTrigger.Clone();
						newTr.StartBoundary = newTr.StartBoundary.Date + new TimeSpan(cron.Hours.vals[h], cron.Minutes.vals[m], 0);
						ret.Add(newTr);
					}
				}
			}
			// Repeating hours and/or minutes
			else if (cron.Minutes.step > 0 || cron.Hours.step > 0)
			{
				int h_start = 0, h_end = 23, m_start = 0, m_end = 59;
				if (cron.Minutes.range)
				{
					m_start = cron.Minutes.vals[0];
					m_end = cron.Minutes.vals[1];
				}
				else if (cron.Minutes.vals.Length == 1)
					m_start = m_end = cron.Minutes.vals[0];

				if (cron.Hours.range)
				{
					h_start = cron.Hours.vals[0];
					h_end = cron.Hours.vals[1];
				}
				else if (cron.Hours.vals.Length == 1)
					h_start = h_end = cron.Hours.vals[0];

				if (h_start == h_end)
				{
					Trigger newTr = (Trigger)baseTrigger.Clone();
					newTr.StartBoundary = newTr.StartBoundary.Date + new TimeSpan(h_start, m_start, 0);
					newTr.Repetition.Interval = TimeSpan.FromMinutes(cron.Minutes.step);
					newTr.Repetition.Duration = TimeSpan.FromHours(1);
					ret.Add(newTr);
				}
				else if (m_start == m_end)
				{
					Trigger newTr = (Trigger)baseTrigger.Clone();
					newTr.StartBoundary = newTr.StartBoundary.Date + new TimeSpan(h_start, m_start, 0);
					newTr.Repetition.Interval = TimeSpan.FromHours(cron.Hours.step);
					newTr.Repetition.Duration = TimeSpan.FromHours(h_end - h_start);
					ret.Add(newTr);
				}
				else
				{
					throw new NotImplementedException();
				}
			}
			return ret;
		}

		private class CronExpression
		{
			public FieldVal[] Fields = new FieldVal[5];

			public CronExpression() { }

			public void Parse(string cronString)
			{
				if (cronString == null)
					throw new ArgumentNullException("cronString");

				var tokens = cronString.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
				if (tokens.Length != 5)
				{
					throw new ArgumentException(string.Format("'{0}' is not a valid crontab expression. It must contain at least 5 components of a schedule "
						+ "(in the sequence of minutes, hours, days, months, days of week).", cronString));
				}

				// min, hr, days, months, daysOfWeek
				for (var i = 0; i < Fields.Length; i++)
				{
					Fields[i] = ParseCronField(tokens[i], (CronFieldType)i);
				}
			}

			public FieldVal Minutes { get { return Fields[0]; } }
			public FieldVal Hours { get { return Fields[1]; } }
			public FieldVal Days { get { return Fields[2]; } }
			public FieldVal Months { get { return Fields[3]; } }
			public FieldVal DOW { get { return Fields[4]; } }

			private FieldVal ParseCronField(string str, CronFieldType cft)
			{
				FieldVal res = new FieldVal();

				if (string.IsNullOrEmpty(str))
					throw new ArgumentNullException("A crontab field value cannot be empty.");

				// Look first for a list of values (e.g. 1,2,3).
				if (str.IndexOf(",") > 0)
				{
					res.vals = Array.ConvertAll<string, int>(str.Split(','), delegate(string s) { return ParseInt(s); });
					res.Validate(cft);
					return res;
				}

				// Look for stepping (e.g. */2 = every 2nd).
				res.step = 1;
				var slashIndex = str.IndexOf("/");
				if (slashIndex > 0)
				{
					res.step = ParseInt(str.Substring(slashIndex + 1));
					str = str.Substring(0, slashIndex);
				}

				// Next, look for wildcard (*).
				if (str.Length == 1 && str[0] == '*')
				{
					res.vals = new int[0];
					res.repeating = true;
					return res;
				}

				// Next, look for a range of values (e.g. 2-10).
				var dashIndex = str.IndexOf("-");
				if (dashIndex > 0)
				{
					var first = ParseInt(str.Substring(0, dashIndex));
					var last = ParseInt(str.Substring(dashIndex + 1));
					if (first >= last)
						throw new ArgumentException("A crontab field value range must begin with a lower number");
					res.range = true;
					res.vals = new int[] { first, last };
					res.Validate(cft);
					return res;
				}

				// Finally, handle the case where there is only one number.
				var value = ParseInt(str);
				res.vals = new int[] { value };
				res.step = 0;
				res.Validate(cft);

				return res;
			}

			private static int ParseInt(string str)
			{
				return int.Parse(str.Trim());
			}

			public enum CronFieldType { Minutes, Hours, Days, Months, DaysOfWeek };

			public struct FieldVal
			{
				public int[] vals;
				public bool repeating, range;
				public int step;

				public bool IsEvery { get { return step == 1 && repeating; } }

				public bool Validate(CronFieldType cft)
				{
					switch (cft)
					{
						case CronFieldType.Minutes:
							return Array.TrueForAll<int>(vals, delegate(int i) { return i >= 0 && i <= 59; });
						case CronFieldType.Hours:
							return Array.TrueForAll<int>(vals, delegate(int i) { return i >= 0 && i <= 23; });
						case CronFieldType.Days:
							return Array.TrueForAll<int>(vals, delegate(int i) { return i >= 1 && i <= 31; });
						case CronFieldType.Months:
							return Array.TrueForAll<int>(vals, delegate(int i) { return i >= 1 && i <= 12; });
						case CronFieldType.DaysOfWeek:
							return Array.TrueForAll<int>(vals, delegate(int i) { return i >= 0 && i <= 6; });
						default:
							break;
					}
					return false;
				}
			}
		}
	}
}

using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cartel {
	public enum TimeUnit {
		Minute, Hour, Day
	}

	public class TimeHelper {
		static float realMinutesPerGameDay = 1f; // Readable

		public static float RealSecondsPerGameDay {
			get {
				return realMinutesPerGameDay * 60f;
			}
		}

		// convert game time (e.g. days or hours) to real user-time (seconds)
		public static float GameTimeToReal(TimeUnit unit, float value) {
			switch (unit) {
				case TimeUnit.Minute:
					return RealSecondsPerGameDay * value / 24 / 60;
				case TimeUnit.Hour:
					return RealSecondsPerGameDay * value / 24;
				case TimeUnit.Day:
					return RealSecondsPerGameDay * value;
			}
			return 0;
		}

		public static String DayProgressToTime(float dayProgress) {
			float time = dayProgress * 24;
			int hours = (int)time;
			int minutes = (int)(60 * (time - (int)time));
			TimeSpan timeSpan = new TimeSpan(hours, minutes, 0);
			return DateTime.Today.Add(timeSpan).ToString("h:mm tt");
		}
	}
}

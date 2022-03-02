using UnityEngine;
using OmiyaGames.Global;

namespace OmiyaGames.Managers
{
	///-----------------------------------------------------------------------
	/// <remarks>
	/// <copyright file="TimeManager.cs" company="Omiya Games">
	/// The MIT License (MIT)
	/// 
	/// Copyright (c) 2014-2022 Omiya Games
	/// 
	/// Permission is hereby granted, free of charge, to any person obtaining a copy
	/// of this software and associated documentation files (the "Software"), to deal
	/// in the Software without restriction, including without limitation the rights
	/// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
	/// copies of the Software, and to permit persons to whom the Software is
	/// furnished to do so, subject to the following conditions:
	/// 
	/// The above copyright notice and this permission notice shall be included in
	/// all copies or substantial portions of the Software.
	/// 
	/// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
	/// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
	/// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
	/// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
	/// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
	/// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
	/// THE SOFTWARE.
	/// </copyright>
	/// <list type="table">
	/// <listheader>
	/// <term>Revision</term>
	/// <description>Description</description>
	/// </listheader>
	/// <item>
	/// <term>
	/// <strong>Date:</strong> 5/18/2015<br/>
	/// <strong>Author:</strong> Taro Omiya
	/// </term>
	/// <description>Initial verison.</description>
	/// </item><item>
	/// <term>
	/// <strong>Version:</strong> 1.0.0-pre.1<br/>
	/// <strong>Date:</strong> 2/12/2022<br/>
	/// <strong>Author:</strong> Taro Omiya
	/// </term>
	/// <description>
	/// Converting from Singleton script to its own unique package.
	/// </description>
	/// </item><item>
	/// <term>
	/// <strong>Version:</strong> 1.0.1-pre.2<br/>
	/// <strong>Date:</strong> 2/12/2022<br/>
	/// <strong>Author:</strong> Taro Omiya
	/// </term>
	/// <description>
	/// Changing to a <c>static</c> class so this script can't be
	/// accidentally attached to another <see cref="GameObject"/>.
	/// Renamed <c>OnBeforeManualPausedChanged</c> to
	/// <see cref="OnBeforeIsManuallyPausedChanged"/>, and
	/// <c>OnAfterManualPausedChanged</c> to
	/// <see cref="OnAfterIsManuallyPausedChanged"/>.
	/// </description>
	/// </item>
	/// </list>
	/// </remarks>
	///-----------------------------------------------------------------------
	/// <summary>
	/// A manager for adjusting the time scale.  Used for manually
	/// pausing the game.  Also allows temporarily slowing down or quickening time,
	/// useful for creating common juicy effects.
	/// </summary>
	public static class TimeManager
	{
		/// <summary>
		/// Grabs the static instance of this manager.
		/// </summary>
		/// <seealso cref="Data"/>
		static Impl GetInstance()
		{
			Impl returnInstance = ComponentSingleton<Impl>.Get(false, out bool isFirstTimeCreated);
			if (isFirstTimeCreated)
			{
				// Grab the saved time scale
				float timeScale = GetDefaultTimeScale();

				// Update the time scales
				returnInstance.Setup();
				returnInstance.TimeScale.Value = timeScale;
			}
			return returnInstance;
		}

		/// <summary>
		/// Triggers when paused.
		/// </summary>
		public static event ITrackable<bool>.ChangeEvent OnBeforeIsManuallyPausedChanged
		{
			add => GetInstance().IsManuallyPaused.OnBeforeValueChanged += value;
			remove => GetInstance().IsManuallyPaused.OnBeforeValueChanged -= value;
		}
		/// <summary>
		/// Triggers when paused.
		/// </summary>
		public static event ITrackable<bool>.ChangeEvent OnAfterIsManuallyPausedChanged
		{
			add => GetInstance().IsManuallyPaused.OnAfterValueChanged += value;
			remove => GetInstance().IsManuallyPaused.OnAfterValueChanged -= value;
		}

	/// <summary>
	/// The "stable" time scale, unaffected by
	/// hit-pauses, etc.
	/// </summary>
	public static float TimeScale
		{
			get => GetInstance().TimeScale.Value;
			set => GetInstance().TimeScale.Value = value;
		}

		/// <summary>
		/// If true, pauses the game.
		/// </summary>
		public static bool IsManuallyPaused
		{
			get => GetInstance().IsManuallyPaused.Value;
			set => GetInstance().IsManuallyPaused.Value = value;
		}

		/// <summary>
		/// Reverts the default time scale stored in
		/// <see cref="Saves.GameSettings"/>.
		/// </summary>
		public static void RevertTimeScale()
		{
			IsManuallyPaused = false;
			TimeScale = GetDefaultTimeScale();
		}

		/// <summary>
		/// Temporarily changes the game's timescale for a short duration.
		/// </summary>
		/// <param name="timeScale">The timescale to set to.</param>
		/// <param name="durationSeconds">
		/// How long the change lasts, in seconds.
		/// Duration is not affected by <seealso cref="Time.timeScale"/>.
		/// </param>
		public static void SetTimeScaleFor(float timeScale, float durationSeconds)
		{
			// Reset TimeManager
			Impl self = GetInstance();
			self.OnDestroy();

			// Start a coroutine
			self.tempTimeScaleChange = Manager.StartCoroutine(self.SetTimeScaleCoroutine(timeScale, durationSeconds));
		}

		static float GetDefaultTimeScale()
		{
			float timeScale = 1f;

			// FIXME: retrieve time-scale through other means
			// Attempt to retrieve the time scale from settings
			Saves.GameSettings settings = Singleton.Get<Saves.GameSettings>();
			if (settings != null)
			{
				timeScale = settings.CustomTimeScale;
			}
			return timeScale;
		}

		class Impl : MonoBehaviour
		{
			TrackTimeScale timeScale = null;
			TrackIsPaused isManuallyPaused = null;
			public Coroutine tempTimeScaleChange = null;

			public ITrackable<float> TimeScale => timeScale;
			public ITrackable<bool> IsManuallyPaused => isManuallyPaused;

			public void OnDestroy()
			{
				if (tempTimeScaleChange != null)
				{
					Manager.StopCoroutine(tempTimeScaleChange);
					tempTimeScaleChange = null;
				}
			}

			public void Setup()
			{
				if (timeScale == null)
				{
					timeScale = new TrackTimeScale();
					timeScale.Update += UpdateTimeScale;
				}
				if (isManuallyPaused == null)
				{
					isManuallyPaused = new TrackIsPaused();
					isManuallyPaused.Update += UpdateTimeScale;
				}
			}

			public System.Collections.IEnumerator SetTimeScaleCoroutine(float tempTimeScale, float durationSeconds)
			{
				// Change the time scale
				Time.timeScale = tempTimeScale;

				// Wait for a short duration
				yield return new WaitForSecondsRealtime(durationSeconds);

				// Revert the time scale
				UpdateTimeScale(TimeScale.Value);
			}

			void UpdateTimeScale(float timeScale) => UpdateTimeScale(timeScale, IsManuallyPaused.Value);
			void UpdateTimeScale(bool isManuallyPaused) => UpdateTimeScale(TimeScale.Value, isManuallyPaused);

			static void UpdateTimeScale(float timeScale, bool isManuallyPaused)
			{
				// Check if paused
				if (isManuallyPaused == false)
				{
					// If not, progress normally
					Time.timeScale = timeScale;
				}
				else
				{
					// If so, pause
					Time.timeScale = 0f;
				}
			}

			class TrackTimeScale : TrackableDecorator<float>
			{
				float timeScale = 1f;

				public event System.Action<float> Update;
				/// <inheritdoc/>
				public override event ITrackable<float>.ChangeEvent OnBeforeValueChanged;
				/// <inheritdoc/>
				public override event ITrackable<float>.ChangeEvent OnAfterValueChanged;

				/// <inheritdoc/>
				public override float Value
				{
					get => timeScale;
					set
					{
						// Clamp value
						if (value < 0f)
						{
							value = 0f;
						}

						// Check if the values are different
						if (Mathf.Approximately(value, timeScale))
						{
							// Halt, if not
							return;
						}

						// Trigger events
						OnBeforeValueChanged?.Invoke(timeScale, value);

						float oldValue = timeScale;
						timeScale = value;
						Update(timeScale);

						OnAfterValueChanged?.Invoke(oldValue, timeScale);
					}
				}
			}

			class TrackIsPaused : TrackableDecorator<bool>
			{
				bool isPaused = false;

				public event System.Action<bool> Update;
				/// <inheritdoc/>
				public override event ITrackable<bool>.ChangeEvent OnBeforeValueChanged;
				/// <inheritdoc/>
				public override event ITrackable<bool>.ChangeEvent OnAfterValueChanged;

				/// <inheritdoc/>
				public override bool Value
				{
					get => isPaused;
					set
					{
						if (isPaused != value)
						{
							OnBeforeValueChanged?.Invoke(isPaused, value);

							bool oldValue = isPaused;
							isPaused = value;
							Update(isPaused);

							OnAfterValueChanged?.Invoke(oldValue, isPaused);
						}
					}
				}
			}
		}
	}
}

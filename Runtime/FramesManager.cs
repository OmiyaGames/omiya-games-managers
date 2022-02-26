using System.Collections;
using UnityEngine;
using OmiyaGames.Global;

namespace OmiyaGames.Managers
{
	///-----------------------------------------------------------------------
	/// <remarks>
	/// <copyright file="FramesManager.cs" company="Omiya Games">
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
	/// <strong>Version:</strong> 1.0.0-pre.1<br/>
	/// <strong>Date:</strong> 2/12/2022<br/>
	/// <strong>Author:</strong> Taro Omiya
	/// </term>
	/// <description>Initial verison.</description>
	/// </item>
	/// </list>
	/// </remarks>
	///-----------------------------------------------------------------------
	/// <summary>
	/// Helper manager to listen to update events, and/or start coroutines.
	/// Useful for runtime scripts that <em>doesn't</em> extend
	/// <see cref="MonoBehaviour"/>, e.g. <see cref="ScriptableObject"/>s.
	/// </summary>
	public class FramesManager : MonoBehaviour
	{
		/// <summary>
		/// Arguments to pass each event call.
		/// </summary>
		public class FrameArgs : System.EventArgs
		{
			/// <summary>
			/// Seconds between each call, affected by
			/// <see cref="Time.timeScale"/>.
			/// </summary>
			public float DeltaTimeScaled
			{
				get;
				internal set;
			}
			/// <summary>
			/// Seconds between each call, unaffected by
			/// <see cref="Time.timeScale"/>.
			/// </summary>
			public float DeltaTimeUnscaled
			{
				get;
				internal set;
			}
			/// <summary>
			/// Seconds that has passed since start of game,
			/// affected by <see cref="Time.timeScale"/>.
			/// </summary>
			public float TimeSinceStartScaled
			{
				get;
				internal set;
			}
			/// <summary>
			/// Seconds that has passed since start of game,
			/// unaffected by <see cref="Time.timeScale"/>.
			/// </summary>
			public float TimeSinceStartUnscaled
			{
				get;
				internal set;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="source"></param>
		/// <param name="scaledDeltaTime"></param>
		/// <param name="unscaledDeltaTime"></param>
		public delegate void EachFrame(FramesManager source, FrameArgs args);
		/// <summary>
		/// Triggers each frame.
		/// </summary>
		public static event EachFrame OnUpdate;
		/// <summary>
		/// Triggers each frame, after
		/// <seealso cref="OnUpdate"/> is finished calling.
		/// </summary>
		public static event EachFrame OnLateUpdate;
		/// <summary>
		/// Triggered every physics update.
		/// </summary>
		public static event EachFrame OnFixedUpdate;

		readonly FrameArgs updateArgs = new(),
			lateUpdateArgs = new(),
			fixedUpdateArgs = new();

		/// <summary>
		/// Starts a coroutine. Useful runtime function if calling from a non-MonoBehavior, or a deactivated one.
		/// </summary>
		/// <param name="coroutine"></param>
		/// <returns></returns>
		public static Coroutine Start(IEnumerator coroutine) => GetInstance().StartCoroutine(coroutine);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="coroutine"></param>
		public static void Stop(Coroutine coroutine) => GetInstance().StopCoroutine(coroutine);

		/// <summary>
		/// 
		/// </summary>
		public static void StopAll() => GetInstance().StopAllCoroutines();

		static FramesManager GetInstance() => ComponentSingleton<FramesManager>.Get(true);

		void Update()
		{
			CallEvent(in OnUpdate, in updateArgs);
		}

		void LateUpdate()
		{
			CallEvent(in OnLateUpdate, in lateUpdateArgs);
		}

		void FixedUpdate()
		{
			CallEvent(in OnFixedUpdate, in fixedUpdateArgs);
		}

		void CallEvent(in EachFrame theEvent, in FrameArgs args)
		{
			if (theEvent != null)
			{
				// Update args
				args.DeltaTimeScaled = Time.deltaTime;
				args.DeltaTimeUnscaled = Time.unscaledDeltaTime;
				args.TimeSinceStartScaled = Time.time;
				args.TimeSinceStartUnscaled = Time.unscaledTime;

				// Call event
				theEvent(this, fixedUpdateArgs);
			}
		}
	}
}

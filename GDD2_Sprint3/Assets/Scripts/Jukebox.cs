﻿/** Jonathan So, jds7523@rit.edu
 * Handles vertical remixing of audio pieces.
 */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class Jukebox : MonoBehaviour {

	public static Jukebox instance; // Singleton Object.

	public GameObject speaker; // The "speaker" object is just a blank prefab with an AudioSource.

	public AudioClip damagedSFX, koSFX;

	// public AudioClip[] audioLayers; // Populate the list of audio layers with audio clips in the order that you want them to be introduced. E.G, the first clip goes first.
	public AudioClip[] audioLayersIntros; // 
	public AudioClip[] audioLayersLoops; // 
	public List<AudioSource> audioSrcs; 
	public AudioClip victoryClip; // Our victory music.

	private const float EPSILON = 0.05f;
	private const int ONE_SEC = 60;

	public void Update() {
		if (Input.GetKeyDown(KeyCode.Alpha1)) {
			AddSpeaker(1);
		} else if (Input.GetKeyDown(KeyCode.Alpha2)) {
			AddSpeaker(2);
		} else if (Input.GetKeyDown(KeyCode.Alpha3)) {
			AddSpeaker(3);
		} else if (Input.GetKeyDown(KeyCode.Alpha4)) {
			AddSpeaker(4);
		}
	}

	/** Fade out the audio on all of our speakers.
	 * Loop through all speakers and call "subspeaker" on them, which'll
	 * lerp their volumes out.
	 */
	public void FadeSpeakers() {
		for (int i = 1; i < audioSrcs.Count; i++) { // We start at one because of the below "Victory" function.
			SubSpeaker(i);
			return;
		}
	}

	/** Forces one of our speakers to play the victory music.
	 * Overrides settings of the speaker at index 0 in audioSrcs 
	 * to play a desired clip.
	 */
	public void Victory() {
		audioSrcs[0].clip = victoryClip;
		audioSrcs[0].volume = 1f;
		audioSrcs[0].Play();
	}

	/** Calls a function to select a speaker to lerp to an audible level. 
	 * Introduce a new channel of audio to the game.
	 * param[speakerIndex] - int; the index of the speaker we want to lerp.
	 */
	public void AddSpeaker(int speakerIndex) {
		StartCoroutine(VRLerp(speakerIndex));
	}

	/** Calls a function to select a speaker to lerp down to volume zero.
	 * "Take out" a channel of audio from the game.
	 * param[speakerIndex] - int; the index of the speaker we want to lerp.
	 */
	public void SubSpeaker(int speakerIndex) {
		StartCoroutine(VRFade(speakerIndex));
	}

	// Play the Damaged SFX.
	public void DamagedSFX() {
		audioSrcs[0].PlayOneShot(damagedSFX);
	}

	// Play the KO'd SFX.
	public void KOSFX() {
		audioSrcs[0].PlayOneShot(koSFX);
	}

	/** Upon awake, make sure that for every audio layer in our music piece, we have
	 * one "speaker" for each.
	 * Also sets up the singleton object.
	 */
	private void Awake() {
		if (instance == null) {
			instance = this;
		}
		// Create as many audio sources as children as there are audio layers.
		for (int i = 0; i < audioLayersIntros.Length; i++) {
			GameObject result = (GameObject) Instantiate(speaker, transform.position, Quaternion.identity);
			result.GetComponent<AudioSource>().clip = audioLayersIntros[i]; // Set its audio clip.
			Debug.Log("Jukebox.Awake() -- nonzero value");
			result.GetComponent<AudioSource>().volume = 0.20f; // Set its volume to zero.
			audioSrcs.Add(result.GetComponent<AudioSource>());
		}
		AddSpeaker(0);
	}

	// Start up the audio loop for the vertical remix.
	public void Start() {
		StartCoroutine("LoopSongVR");		
	}


	/** Wait in the frozen time, if we're ever modifying the time scale.
	* Probably stolen from some tutorial on the internet.
	* param[time] - float that expresses how long to wait.
	*/
	private IEnumerator WaitForRealSeconds(float time) {
		float init = Time.realtimeSinceStartup;
		while (Time.realtimeSinceStartup < init + time) {
			yield return null;
		}
	}

	/** Infinitely loops the layers of the audio.
	 * Tells all speaker objects to play their respective audio clip.
	 * Everything is based on the length of the first one, however.
	 */
	private IEnumerator LoopSongVR () {
		// Play the introductions
		for (int i = 0; i < audioLayersIntros.Length; i++) {
			audioSrcs[i].Play();
		}
		// Wait until the first speaker is done with their audio clip before looping.
		yield return StartCoroutine(WaitForRealSeconds(audioLayersIntros[0].length));
		while (audioSrcs[0].isPlaying) {
			yield return new WaitForEndOfFrame();
		}

		// Now, swap all audio files with the looped versions before proceeding.
		for (int i = 0; i < audioLayersLoops.Length; i++) {
			audioSrcs[i].clip = audioLayersLoops[i];
		}

		// Play the looped portion of each track.
		while (true) {
			// Play all layers of audio
			for (int i = 0; i < audioLayersLoops.Length; i++) {
				audioSrcs[i].Play();
			}
			// Wait until the first speaker is done with their audio clip before looping.
			yield return StartCoroutine(WaitForRealSeconds(audioLayersLoops[0].length));
			while (audioSrcs[0].isPlaying) {
				yield return new WaitForEndOfFrame();
			}
		}
	}

	/** Lerps a selected speaker to become audible over one second. 
	 * Gradually lerps a speaker's volume to become 1 / (the number of channels we have).
	 * param[speakerIndex] - int; the index of the speaker we want to lerp.
	 */
	private IEnumerator VRLerp(int speakerIndex) {
		float timer = 0;
		AudioSource currSpeaker = audioSrcs[speakerIndex];
		for (int i = 0; i < ONE_SEC; i++) {
			currSpeaker.volume = Mathf.Lerp(0, (1f / audioLayersIntros.Length), timer);
			timer += Time.fixedDeltaTime;
			yield return new WaitForSeconds(Time.fixedDeltaTime);
		}
	}

	/** Lerps a selected speaker to become inaudible over one second. 
	 * Gradually lerps a speaker's volume to become 0.
	 * param[speakerIndex] - int; the index of the speaker we want to lerp.
	 */
	private IEnumerator VRFade(int speakerIndex) {
		float timer = 0;
		AudioSource currSpeaker = audioSrcs[speakerIndex];
		for (int i = 0; i < ONE_SEC; i++) {
			currSpeaker.volume = Mathf.Lerp((1f / audioLayersIntros.Length), 0, timer);
			timer += Time.fixedDeltaTime;
			yield return new WaitForSeconds(Time.fixedDeltaTime);
		}
	}
}


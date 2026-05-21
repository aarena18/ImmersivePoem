using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorFunctions : MonoBehaviour
{
	[SerializeField] MenuButtonController menuButtonController;

	[Tooltip("Index de ce bouton : 0 = Commencer, 1 = Paramètres, 2 = Quitter")]
	[SerializeField] int buttonIndex;

	public bool disableOnce;

	void PlaySound(AudioClip whichSound)
	{
		if (!disableOnce)
		{
			// Priorité à la voix dynamique (femme / batman)
			if (MenuSoundManager.Instance != null)
			{
				AudioClip voiceClip = MenuSoundManager.Instance.GetClipForButton(buttonIndex);
				if (voiceClip != null)
				{
					menuButtonController.audioSource.PlayOneShot(voiceClip);
					return;
				}
			}
			// Fallback : son original passé par l'animation event
			menuButtonController.audioSource.PlayOneShot(whichSound);
		}
		else
		{
			disableOnce = false;
		}
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Photon.Pun;
using UnityEngine;

public class DrainUsable : GenericUsable {
    [SerializeField] private AudioPack drainSound;
    [SerializeField] private GenericReagentContainer drainContainer;
    [SerializeField] private Sprite displaySprite;
    private AudioSource audioSource;
    private void Start() {
        if (audioSource == null) {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.maxDistance = 10f;
            audioSource.minDistance = 0.2f;
            audioSource.rolloffMode = AudioRolloffMode.Linear;
            audioSource.spatialBlend = 1f;
            audioSource.loop = true;
            audioSource.enabled = false;
        }
    }

    private bool draining;
    public override Sprite GetSprite(Kobold k) {
        return displaySprite;
    }

    public override bool CanUse(Kobold k) {
        return base.CanUse(k) && drainContainer.volume > 0.01f;
    }

    public override void Use() {
        base.Use();
        StartCoroutine(Drain());
    }

    IEnumerator Drain() {
        draining = true;
        audioSource.enabled = true;
        drainSound.Play(audioSource);
        while (drainContainer.volume > 0.01f) {
            drainContainer.Spill(Time.deltaTime * 10f);
            yield return null;
        }
        audioSource.enabled = false;
        draining = false;
    }

    public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        base.OnPhotonSerializeView(stream, info);
        if (stream.IsWriting) {
            stream.SendNext(draining);
        } else {
            bool newDraining = (bool)stream.ReceiveNext();
            if (!draining && newDraining) {
                StartCoroutine(Drain());
            }
            draining = newDraining;
        }
    }

    public override void Load(BinaryReader reader, string version) {
        base.Load(reader, version);
        bool newDraining = reader.ReadBoolean();
        if (!draining && newDraining) {
            StartCoroutine(Drain());
        }
        draining = newDraining;
    }

    public override void Save(BinaryWriter writer, string version) {
        base.Save(writer, version);
        writer.Write(draining);
    }
}

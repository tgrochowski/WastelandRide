﻿using System;
using UnityEngine;
using VehiclePackage;

namespace CharacterPackage.Scripts
{
	[RequireComponent(typeof(Rigidbody))]
	[RequireComponent(typeof(Animator))]
	[RequireComponent(typeof(CapsuleCollider))]
	public class HumanoidRagdollBehaviour : MonoBehaviour {
		private Collider mainCollider;
		private Collider[] allColliders;
		private Rigidbody[] bodies;
		private Animator anim;
		private Rigidbody rb;
		public float forceMultiplier = 0.4f;
		public Rigidbody ragdollRigidbody;
		private bool ragdoll;
		private Vector3 hitForce;

		

		private void Awake()
		{
			ragdoll = false;
			mainCollider = GetComponent<Collider>();
			allColliders = GetComponentsInChildren<Collider>(true);
			bodies = GetComponentsInChildren<Rigidbody>(true);
			anim = GetComponent<Animator>();
			rb = GetComponent<Rigidbody>();
			DoRagdoll(false);
			if (ragdollRigidbody == null)
			{
				ragdollRigidbody = anim.GetBoneTransform(HumanBodyBones.Hips).GetComponent<Rigidbody>();
			}
		}

		public Rigidbody GetRb()
		{
			return rb;
		}

		public void DoRagdoll(bool isRagdoll)
		{
			foreach (var col in allColliders)
			{
				col.enabled = isRagdoll;
			}

			mainCollider.enabled = !isRagdoll;
			rb.useGravity = !isRagdoll;
			rb.isKinematic = isRagdoll;
			anim.enabled = !isRagdoll;
		}

		private void OnCollisionEnter (Collision collision) {
			TriggerVehicleHit(collision);
		}


		private void TriggerVehicleHit(Collision collision)
		{
			var col = collision.collider;
			if (!col.CompareTag("Vehicle")) return;
			
			var colRigidbody = col.GetComponent<Rigidbody>();
			var force = colRigidbody.velocity.magnitude;

			if (Math.Abs(force) < 6) return;
			ContactPoint contact = collision.contacts[0];
			
			var vehiclePos = col.transform.position;
			var collisionPos = contact.point;
			
			Vector3 normalizedVehiclePos = new Vector3(vehiclePos.x,collisionPos.y,vehiclePos.z);
			var hitDirection =  (collisionPos - normalizedVehiclePos).normalized;
			hitForce = hitDirection*force*forceMultiplier;
			
			Debug.Log(hitForce);
			hitForce.y = 0.1f;
			ragdoll = true;
		}

		private void FixedUpdate()
		{
			//prevent falling through floor
			if (ragdoll)
			{
				//TODO:: WHY THE FORCE IS THE SAME
				ragdollRigidbody.velocity = Vector3.zero;
				DoRagdoll(true);
				ragdollRigidbody.velocity = Vector3.zero;
				//log
				Debug.Log("NPC was hit");
				ragdollRigidbody.AddForce(hitForce, ForceMode.Impulse);
				ragdoll = false;
			}
		}
	}
}

/*==============================================================================
Copyright (c) 2021, PTC Inc. All rights reserved.
Vuforia is a trademark of PTC Inc., registered in the United States and other countries.
==============================================================================*/

using System.Collections.Generic;
using UnityEngine;
using Vuforia;

public class MultiArea : MonoBehaviour
{
    #region PRIVATE_MEMBER_VARS

    /// <summary>
    ///     Trackable poses relative to the MultiArea root
    /// </summary>
    private readonly Dictionary<string, Matrix4x4> mPoses = new();

    private bool m_Tracked;

    #endregion PRIVATE_MEMBER_VARS


    #region UNITY_MONOBEHAVIOUR_METHODS

    // Start is called before the first frame update
    private void Start()
    {
        var areaTargets = GetComponentsInChildren<AreaTargetBehaviour>(true);
        foreach (var at in areaTargets)
        {
            // Remember the relative pose of each AT to the group root node
            var matrix = GetFromToMatrix(at.transform, transform);
            mPoses[at.TargetName] = matrix;
            Debug.Log("Original pose: " + at.TargetName + "\n" + matrix.ToString(""));

            // Detach augmentation and re-parent it under the group root node
            for (var i = at.transform.childCount - 1; i >= 0; i--)
            {
                var child = at.transform.GetChild(i);
                child.SetParent(transform, true);
            }
        }
    }

    // Update is called once per frame
    private void Update()
    {
        if (!VuforiaApplication.Instance.IsRunning) return;

        // Check if an Area Target is tracked
        var atb = GetBestTrackedAreaTarget();

        if (atb != null) // When an Area Target is detected
        {
            if (!m_Tracked) m_Tracked = true;

            if (GetGroupPoseFromAreaTarget(atb, out var groupPose))
            {
                transform.position = groupPose.GetColumn(3);
                transform.rotation = Quaternion.LookRotation(groupPose.GetColumn(2), groupPose.GetColumn(1));
            }
        }
        else // No Area Target is detected
        {
            if (m_Tracked) m_Tracked = false;
        }
    }

    #endregion UNITY_MONOBEHAVIOUR_METHODS


    #region PRIVATE_METHODS

    private AreaTargetBehaviour GetBestTrackedAreaTarget()
    {
        var trackedAreaTargets = GetTrackedAreaTargets(true);
        if (trackedAreaTargets.Count == 0) return null;

        // look for extended/tracked targets
        foreach (var at in trackedAreaTargets)
            if (at.TargetStatus.Status == Status.TRACKED ||
                at.TargetStatus.Status == Status.EXTENDED_TRACKED)
                return at;

        // if no target in EXT/TRACKED was found,
        // then fallback to any other target
        // i.e. including LIMITED ones;
        // just report the first in the list
        return trackedAreaTargets[0];
    }

    private List<AreaTargetBehaviour> GetTrackedAreaTargets(bool includeLimited = false)
    {
        var trackedTargets = new List<AreaTargetBehaviour>();
        var activeAreaTargets = FindObjectsOfType<AreaTargetBehaviour>();
        foreach (var target in activeAreaTargets)
            if (target.enabled &&
                (target.TargetStatus.Status == Status.TRACKED ||
                 target.TargetStatus.Status == Status.EXTENDED_TRACKED ||
                 (includeLimited && target.TargetStatus.Status == Status.LIMITED)))
                trackedTargets.Add(target);
        return trackedTargets;
    }

    private bool GetGroupPoseFromAreaTarget(AreaTargetBehaviour atb, out Matrix4x4 groupPose)
    {
        groupPose = Matrix4x4.identity;
        if (mPoses.TryGetValue(atb.TargetName, out var areaTargetToGroup))
        {
            // Matrix of group root node w.r.t. AT
            var groupToAreaTarget = areaTargetToGroup.inverse;

            // Current atb matrix
            var areaTargetToWorld = atb.transform.localToWorldMatrix;
            groupPose = areaTargetToWorld * groupToAreaTarget;
            return true;
        }

        return false;
    }

    private static Matrix4x4 GetFromToMatrix(Transform from, Transform to)
    {
        var m1 = from ? from.localToWorldMatrix : Matrix4x4.identity;
        var m2 = to ? to.worldToLocalMatrix : Matrix4x4.identity;
        return m2 * m1;
    }

    #endregion PRIVATE_METHODS
}
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;

public class NavMeshUtilityTests
{
    private int CURRENT_ID = 0;

    [Test]
    public void FacingUp()
    {
        AssertFacingUp(Vector3(0, 0, 2), Vector3(0, 0, 0), Vector3(2, 0, 0), false, "D");

        AssertFacingUp(Vector3(0, 0, 0), Vector3(0, 0, 2), Vector3(2, 0, 0), true, "A");
        AssertFacingUp(Vector3(0, 0, 2), Vector3(2, 0, 0), Vector3(0, 0, 0), true, "B");
        AssertFacingUp(Vector3(2, 0, 0), Vector3(0, 0, 0), Vector3(0, 0, 2), true, "C");
    }
    
    private Vector3 Vector3(float x, float y, float z)
    {
        return new Vector3(x, y, z);
    }

    private void AssertFacingUp(Vector3 a, Vector3 b, Vector3 c, bool isFacingUp, string Tag)
    {
        if (NavMeshUtility.IsFacingUp(a, b, c) && !isFacingUp)
        {
            Assert.Fail(Tag + " should not be facing Up.");
        }
        else if (!NavMeshUtility.IsFacingUp(a, b, c) && isFacingUp)
        {
            Assert.Fail(Tag + " should be facing Up.");
        }
    }
}

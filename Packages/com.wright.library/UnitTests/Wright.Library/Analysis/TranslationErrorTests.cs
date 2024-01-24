using NUnit.Framework;
using UnityEngine;
using Wright.Library.Analysis;

namespace UnitTests.Wright.Library.Analysis
{
    public class TranslationErrorTests
    {
        [Test]
        public void GetError_OnTheSameMesh_ReturnsZero()
        {
            var goalMesh = MakeGoalMesh();
            var generatedMesh = MakeGoalMesh();

            var result = TranslationError.GetError(goalMesh, generatedMesh);

            Assert.That(result, Is.EqualTo(0));
        }
        
        [Test]
        public void GetError_WhenMeshesDontLineUp_ThrowException()
        {
            var goalMesh = MakeGoalMesh();
            var generatedMesh = LargerMesh();

            Assert.Throws<UnityEngine.Assertions.AssertionException>(() =>
            {
                TranslationError.GetError(goalMesh, generatedMesh);
            });
        }
        
        // [Test]
        // public void GetError_OnMeshMovedOneVertex_ReturnsThatError()
        // {
        //     var goalMesh = MakeGoalMesh();
        //     var generatedMesh = MakeOneMoved();
        //
        //     var result = TranslationError.GetError(goalMesh, generatedMesh);
        //
        //     Assert.That(result, Is.EqualTo(1));
        // }

        #region Helpers

        private static Mesh MakeMesh(Vector3[] vertices, int[] triangles)
        {
            var mesh = new Mesh
            {
                vertices = vertices,
                triangles = triangles
            };

            mesh.RecalculateNormals();

            return mesh;
        }

        private static Mesh MakeGoalMesh()
        {
            var vertices = new Vector3[]
            {
                new(1, 0, 1),
                new(1.5f, 1.5f, 0),
                new(2, 0, -1)
            };

            var triangles = new[]
            {
                0, 1, 2
            };

            return MakeMesh(vertices, triangles);
        }

        private static Mesh MakeOneMoved()
        {
            var mesh = MakeGoalMesh();

            mesh.vertices[0].x += 1;

            return mesh;
        }
        
        private static Mesh LargerMesh()
        {
            var vertices = new Vector3[]
            {
                new(1, 0, 1),
                new(1.5f, 1.5f, 0),
                new(2, 0, -1),
                Vector3.zero, 
            };

            var triangles = new[]
            {
                0, 1, 2,
                1, 2, 3
            };

            return MakeMesh(vertices, triangles);
        }

        #endregion
    }
}
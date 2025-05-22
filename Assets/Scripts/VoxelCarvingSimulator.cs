using System;
using UnityEngine;

namespace VoxelCarving
{

	public enum CleanupMode : uint
	{
		None = 0,
		KeepLargestComponent = 1,
		KeepGuardVoxel = 2
	}

	public enum MeshGenerationMode : uint
	{
		Simple = 0,
		MarchingCubes = 1
	}

	public enum SmoothMode : uint
	{
		None = 0,
		Averaging = 1,
		AveragingSeparable = 2,
		Gaussian = 3,
		GaussianSeparable = 4
	}

	public delegate void LoggingCallback(string message);

	public class VoxelCarvingSimulator
	{
		public static void setLoggingCallback(LoggingCallback loggingCallback)
		{
			NativeMethods.VoxelCarving_setLoggingCallback(loggingCallback);
		}

		public VoxelCarvingSimulator()
		{
			m_handle = NativeMethods.VoxelCarving_create();

			if (m_handle == IntPtr.Zero)
			{
				throw new Exception("Could not create voxel carving simulator");
			}
		}

		public void destroy()
		{
			NativeMethods.VoxelCarving_destroy(m_handle);

			m_handle = IntPtr.Zero;
		}

		public void setSolid(int voxelsX, int voxelsY, int voxelsZ, float voxelSize)
		{
			if(NativeMethods.VoxelCarving_setSolid(m_handle, voxelsX, voxelsY, voxelsZ, voxelSize) == 0)
			{
				throw new Exception("Could not set solid");
			}
		}

		public void setSolid(Vector3[] vertices, int[] triangles, float voxelSize)
		{
			if (triangles.Length % 3 != 0)
			{
				throw new Exception("Invalid array length");
			}

			if (NativeMethods.VoxelCarving_setSolidEx(m_handle, vertices, triangles, vertices.Length, triangles.Length / 3, voxelSize) == 0)
			{
				throw new Exception("Could not set solid");
			}
		}

		public void setCleanupMode(CleanupMode cleanupMode)
		{
			if (NativeMethods.VoxelCarving_setCleanupMode(m_handle, cleanupMode) == 0)
			{
				throw new Exception("Could not set cleanup mode");
			}
		}

		public void setGuardVoxel(int x, int y, int z)
		{
			if (NativeMethods.VoxelCarving_setGuardVoxel(m_handle, x, y, z) == 0)
			{
				throw new Exception("Could not set guard voxel");
			}
		}

		public void setMeshGenerationMode(MeshGenerationMode meshGenerationMode)
		{
			if (NativeMethods.VoxelCarving_setMeshGenerationMode(m_handle, meshGenerationMode) == 0)
			{
				throw new Exception("Could not set mesh generation mode");
			}
		}

		public void setCells(int cellsX, int cellsY, int cellsZ)
		{
			if (NativeMethods.VoxelCarving_setCells(m_handle, cellsX, cellsY, cellsZ) == 0)
			{
				throw new Exception("Could not set cells");
			}
		}

		public void setThreshold(float threshold)
		{
			if (NativeMethods.VoxelCarving_setThreshold(m_handle, threshold) == 0)
			{
				throw new Exception("Could not set threshold");
			}
		}

		public void setSmoothMode(SmoothMode smoothMode)
		{
			if (NativeMethods.VoxelCarving_setSmoothMode(m_handle, smoothMode) == 0)
			{
				throw new Exception("Could not set smooth mode");
			}
		}

		public void setKernelSize(int kernelSize)
		{
			if (NativeMethods.VoxelCarving_setKernelSize(m_handle, kernelSize) == 0)
			{
				throw new Exception("Could not set kernel size");
			}
		}

		public void setSigma(float sigma)
		{
			if (NativeMethods.VoxelCarving_setSigma(m_handle, sigma) == 0)
			{
				throw new Exception("Could not set sigma");
			}
		}

		public int setToolVoxels(int tool, Vector3[] vertices, int[] triangles, float voxelSize, bool active)
		{
			if (triangles.Length % 3 != 0)
			{
				throw new Exception("Invalid array length");
			}

			int voxels = NativeMethods.VoxelCarving_setToolVoxels(m_handle, tool, vertices, triangles, vertices.Length, triangles.Length / 3, voxelSize, active);

			if (voxels == 0)
			{
				throw new Exception("Could not set tool voxels");
			}

			return voxels;
		}

		public int setToolConvex(int tool, Vector3[] vertices, int vertexLimit, int polygonLimit, bool active)
		{
			int planes = NativeMethods.VoxelCarving_setToolConvex(m_handle, tool, vertices, vertices.Length, vertexLimit, polygonLimit, active);

			if (planes == 0)
			{
				throw new Exception("Could not set tool convex");
			}

			return planes;
		}

		public bool isToolActive(int tool)
		{
			bool active;

			if (NativeMethods.VoxelCarving_isToolActive(m_handle, tool, out active) == 0)
			{
				throw new Exception("Could not retrieve tool state");
			}

			return active;
		}

		public void activateTool(int tool, bool active)
		{
			if (NativeMethods.VoxelCarving_activateTool(m_handle, tool, active) == 0)
			{
				throw new Exception("Could not set tool state");
			}
		}

		public void setSolidTransform(Matrix4x4 localToWorldMatrix)
		{
			if (NativeMethods.VoxelCarving_setSolidTransform(m_handle, ref localToWorldMatrix) == 0)
			{
				throw new Exception("Could not set solid transform");
			}
		}

		public void setToolTransform(int tool, Matrix4x4 localToWorldMatrix)
		{
			if (NativeMethods.VoxelCarving_setToolTransform(m_handle, tool, ref localToWorldMatrix) == 0)
			{
				throw new Exception("Could not set tool transform");
			}
		}

		public bool simulate()
		{
			bool result;

			if (NativeMethods.VoxelCarving_simulate(m_handle, out result) == 0)
			{
				throw new Exception("Simulation error");
			}

			return result;
		}

		public void getSolidVisualizationMesh(out Vector3[] vertices, out Vector3[] normals, out int[] triangles)
		{
			int numVertices;
			int numTriangles;

			if (NativeMethods.VoxelCarving_getSolidVisualizationMeshSize(m_handle, out numVertices, out numTriangles) == 0)
			{
				throw new Exception("Could not retrieve solid visualization mesh size");
			}

			vertices = new Vector3[numVertices];
			normals = new Vector3[numVertices];
			triangles = new int[numTriangles * 3];

			if (NativeMethods.VoxelCarving_getSolidVisualizationMesh(m_handle, vertices, normals, triangles) == 0)
			{
				throw new Exception("Could not retrieve solid visualization mesh");
			}
		}

		public void getToolVisualizationMesh(int tool, out Vector3[] vertices, out Vector3[] normals, out int[] triangles)
		{
			int numVertices;
			int numTriangles;

			if (NativeMethods.VoxelCarving_getToolVisualizationMeshSize(m_handle, tool, out numVertices, out numTriangles) == 0)
			{
				throw new Exception("Could not retrieve tool visualization mesh size");
			}

			vertices = new Vector3[numVertices];
			normals = new Vector3[numVertices];
			triangles = new int[numTriangles * 3];

			if (NativeMethods.VoxelCarving_getToolVisualizationMesh(m_handle, tool, vertices, normals, triangles) == 0)
			{
				throw new Exception("Could not retrieve tool visualization mesh");
			}
		}

		public void saveSolid(string filename)
		{
			if (NativeMethods.VoxelCarving_saveSolid(m_handle, filename) == 0)
			{
				throw new Exception("Could not save solid");
			}
		}

		public void loadSolid(string filename)
		{
			if (NativeMethods.VoxelCarving_loadSolid(m_handle, filename) == 0)
			{
				throw new Exception("Could not load solid");
			}
		}

		IntPtr m_handle = IntPtr.Zero;
	}
}

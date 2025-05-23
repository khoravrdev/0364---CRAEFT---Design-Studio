using System;
using System.Runtime.InteropServices;

namespace VoxelCarving
{
	internal class NativeMethods
	{
		//internal const string DLL_FILENAME = "VoxelCarvingCpu.dll";
		internal const string DLL_FILENAME = "VoxelCarvingGpu.dll";

		[DllImport(DLL_FILENAME, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void VoxelCarving_setLoggingCallback(LoggingCallback pLoggingCallback);

		[DllImport(DLL_FILENAME, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr VoxelCarving_create();

		[DllImport(DLL_FILENAME, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void VoxelCarving_destroy(IntPtr voxelCarving);

		[DllImport(DLL_FILENAME, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int VoxelCarving_setSolid(IntPtr voxelCarving, int voxelsX, int voxelsY, int voxelsZ, float voxelSize);

		[DllImport(DLL_FILENAME, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int VoxelCarving_setSolidEx(IntPtr voxelCarving, UnityEngine.Vector3[] vertices, int[] triangles, int numVertices, int numTriangles, float voxelSize);

		[DllImport(DLL_FILENAME, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int VoxelCarving_setCleanupMode(IntPtr voxelCarving, CleanupMode cleanupMode);

		[DllImport(DLL_FILENAME, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int VoxelCarving_setGuardVoxel(IntPtr voxelCarving, int x, int y, int z);

		[DllImport(DLL_FILENAME, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int VoxelCarving_setMeshGenerationMode(IntPtr voxelCarving, MeshGenerationMode meshGenerationMode);

		[DllImport(DLL_FILENAME, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int VoxelCarving_setCells(IntPtr voxelCarving, int cellsX, int cellsY, int cellsZ);

		[DllImport(DLL_FILENAME, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int VoxelCarving_setThreshold(IntPtr voxelCarving, float threshold);

		[DllImport(DLL_FILENAME, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int VoxelCarving_setSmoothMode(IntPtr voxelCarving, SmoothMode smoothMode);

		[DllImport(DLL_FILENAME, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int VoxelCarving_setKernelSize(IntPtr voxelCarving, int kernelSize);

		[DllImport(DLL_FILENAME, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int VoxelCarving_setSigma(IntPtr voxelCarving, float sigma);

		[DllImport(DLL_FILENAME, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int VoxelCarving_setToolVoxels(IntPtr voxelCarving, int tool, UnityEngine.Vector3[] vertices, int[] triangles, int numVertices, int numTriangles, float voxelSize, [MarshalAs(UnmanagedType.U1)] bool active);

		[DllImport(DLL_FILENAME, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int VoxelCarving_setToolConvex(IntPtr voxelCarving, int tool, UnityEngine.Vector3[] vertices, int numVertices, int vertexLimit, int polygonLimit, [MarshalAs(UnmanagedType.U1)] bool active);

		[DllImport(DLL_FILENAME, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int VoxelCarving_isToolActive(IntPtr voxelCarving, int tool, [MarshalAs(UnmanagedType.U1)] out bool active);

		[DllImport(DLL_FILENAME, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int VoxelCarving_activateTool(IntPtr voxelCarving, int tool, [MarshalAs(UnmanagedType.U1)] bool active);

		[DllImport(DLL_FILENAME, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int VoxelCarving_setSolidTransform(IntPtr voxelCarving, ref UnityEngine.Matrix4x4 localToWorldMatrix);

		[DllImport(DLL_FILENAME, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int VoxelCarving_setToolTransform(IntPtr voxelCarving, int tool, ref UnityEngine.Matrix4x4 localToWorldMatrix);

		[DllImport(DLL_FILENAME, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int VoxelCarving_simulate(IntPtr voxelCarving, [MarshalAs(UnmanagedType.U1)] out bool result);

		[DllImport(DLL_FILENAME, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int VoxelCarving_getSolidVisualizationMeshSize(IntPtr voxelCarving, out int numVertices, out int numTriangles);

		[DllImport(DLL_FILENAME, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int VoxelCarving_getSolidVisualizationMesh(IntPtr voxelCarving, [Out] UnityEngine.Vector3[] vertices, [Out] UnityEngine.Vector3[] normals, [Out] int[] triangles);

		[DllImport(DLL_FILENAME, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int VoxelCarving_getToolVisualizationMeshSize(IntPtr voxelCarving, int tool, out int numVertices, out int numTriangles);

		[DllImport(DLL_FILENAME, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int VoxelCarving_getToolVisualizationMesh(IntPtr voxelCarving, int tool, [Out] UnityEngine.Vector3[] vertices, [Out] UnityEngine.Vector3[] normals, [Out] int[] triangles);

		[DllImport(DLL_FILENAME, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int VoxelCarving_saveSolid(IntPtr voxelCarving, string filename);

		[DllImport(DLL_FILENAME, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int VoxelCarving_loadSolid(IntPtr voxelCarving, string filename);
	}
}

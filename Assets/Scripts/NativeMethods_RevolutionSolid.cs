using System;
using System.Runtime.InteropServices;

namespace RevolutionSolid
{
	/// <summary>
	/// Methods used to bridge the C procedures exposed by the DLL and the Generator/Voxelizer C# class
	/// </summary>
	class NativeMethods
	{
		/// <summary>
		/// Update this in case you change the DLL's name or path
		/// </summary>
		const string DLL_FILENAME = "RevolutionSolid.dll";

		[DllImport(DLL_FILENAME, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void RevolutionSolid_setLoggingCallback(LoggingCallback loggingCallback);

		[DllImport(DLL_FILENAME, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr RevolutionSolid_create();

		[DllImport(DLL_FILENAME, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void RevolutionSolid_destroy(IntPtr potteryGenerator);

		[DllImport(DLL_FILENAME, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void RevolutionSolid_setTemplate(IntPtr potteryGenerator, string filename);

		[DllImport(DLL_FILENAME, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void RevolutionSolid_setTemplateEx(IntPtr potteryGenerator, string filename, int guardPixelX, int guardPixelY);

		[DllImport(DLL_FILENAME, CallingConvention = CallingConvention.Cdecl)]
		internal static extern string RevolutionSolid_getTemplate(IntPtr potteryGenerator);

		[DllImport(DLL_FILENAME, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void RevolutionSolid_saveAsTemplate(IntPtr potteryGenerator, string filename);

		[DllImport(DLL_FILENAME, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void RevolutionSolid_setResolution(IntPtr potteryGenerator, int resolution);

		[DllImport(DLL_FILENAME, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int RevolutionSolid_getResolution(IntPtr potteryGenerator);

		[DllImport(DLL_FILENAME, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void RevolutionSolid_setSlices(IntPtr potteryGenerator, int slices);

		[DllImport(DLL_FILENAME, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int RevolutionSolid_getSlices(IntPtr potteryGenerator);

		[DllImport(DLL_FILENAME, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void RevolutionSolid_enableSmoothing(IntPtr potteryGenerator, [MarshalAs(UnmanagedType.U1)] bool enable);

		[DllImport(DLL_FILENAME, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.U1)]
		internal static extern bool RevolutionSolid_isSmoothingEnabled(IntPtr potteryGenerator);

		[DllImport(DLL_FILENAME, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void RevolutionSolid_setSmoothingKernelSize(IntPtr potteryGenerator, int smoothingKernelSize);

		[DllImport(DLL_FILENAME, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int RevolutionSolid_getSmoothingKernelSize(IntPtr potteryGenerator);

		[DllImport(DLL_FILENAME, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void RevolutionSolid_setSmoothingKernelDeviation(IntPtr potteryGenerator, float smoothingKernelDeviation);

		[DllImport(DLL_FILENAME, CallingConvention = CallingConvention.Cdecl)]
		internal static extern float RevolutionSolid_getSmoothingKernelDeviation(IntPtr potteryGenerator);

		[DllImport(DLL_FILENAME, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void RevolutionSolid_enableVisualization(IntPtr potteryGenerator, [MarshalAs(UnmanagedType.U1)] bool enable);

		[DllImport(DLL_FILENAME, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.U1)]
		internal static extern bool RevolutionSolid_isVisualizationEnabled(IntPtr potteryGenerator);

		[DllImport(DLL_FILENAME, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void RevolutionSolid_setVisualizationFlags(IntPtr potteryGenerator, VisualizationFlags visualizationFlags);

		[DllImport(DLL_FILENAME, CallingConvention = CallingConvention.Cdecl)]
		internal static extern VisualizationFlags RevolutionSolid_getVisualizationFlags(IntPtr potteryGenerator);

		[DllImport(DLL_FILENAME, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void RevolutionSolid_addTool(IntPtr potteryGenerator, int tool, ref UnityEngine.Matrix4x4 localToWorldMatrix, ToolType toolType, [MarshalAs(UnmanagedType.U1)] bool active);

		[DllImport(DLL_FILENAME, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void RevolutionSolid_addMultiTool(IntPtr potteryGenerator, int tool, UnityEngine.Vector3[] voxelCenters, int numVoxelCenters, float voxelSize, ref UnityEngine.Matrix4x4 localToWorldMatrix, ToolType toolType, [MarshalAs(UnmanagedType.U1)] bool active);

		[DllImport(DLL_FILENAME, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void RevolutionSolid_removeTool(IntPtr potteryGenerator, int tool);

		[DllImport(DLL_FILENAME, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void RevolutionSolid_removeAllTools(IntPtr potteryGenerator);

		[DllImport(DLL_FILENAME, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void RevolutionSolid_activateTool(IntPtr potteryGenerator, int tool, [MarshalAs(UnmanagedType.U1)] bool active);

		[DllImport(DLL_FILENAME, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void RevolutionSolid_deactivateAllTools(IntPtr potteryGenerator);

		[DllImport(DLL_FILENAME, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.U1)]
		internal static extern bool RevolutionSolid_isToolActive(IntPtr potteryGenerator, int tool);

		[DllImport(DLL_FILENAME, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void RevolutionSolid_setToolType(IntPtr potteryGenerator, int tool, ToolType toolType);

		[DllImport(DLL_FILENAME, CallingConvention = CallingConvention.Cdecl)]
		internal static extern ToolType RevolutionSolid_getToolType(IntPtr potteryGenerator, int tool);

		[DllImport(DLL_FILENAME, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void RevolutionSolid_setToolLocalToWorldMatrix(IntPtr potteryGenerator, int tool, ref UnityEngine.Matrix4x4 localToWorldMatrix);

		[DllImport(DLL_FILENAME, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void RevolutionSolid_getToolLocalToWorldMatrix(IntPtr potteryGenerator, int tool, out UnityEngine.Matrix4x4 localToWorldMatrix);

		[DllImport(DLL_FILENAME, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void RevolutionSolid_setSolidWorldToLocalMatrix(IntPtr potteryGenerator, ref UnityEngine.Matrix4x4 worldToLocalMatrix);

		[DllImport(DLL_FILENAME, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void RevolutionSolid_getSolidWorldToLocalMatrix(IntPtr potteryGenerator, out UnityEngine.Matrix4x4 worldToLocalMatrix);

		[DllImport(DLL_FILENAME, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void RevolutionSolid_getWorldBounds(IntPtr potteryGenerator, out UnityEngine.Vector3 worldBoundsMin, out UnityEngine.Vector3 worldBoundsMax);

		[DllImport(DLL_FILENAME, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void RevolutionSolid_pushUndo(IntPtr potteryGenerator);

		[DllImport(DLL_FILENAME, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void RevolutionSolid_popUndo(IntPtr potteryGenerator);

		[DllImport(DLL_FILENAME, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void RevolutionSolid_clearUndoStack(IntPtr potteryGenerator);

		[DllImport(DLL_FILENAME, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void RevolutionSolid_restart(IntPtr potteryGenerator);

		[DllImport(DLL_FILENAME, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.U1)]
		internal static extern bool RevolutionSolid_generate(IntPtr potteryGenerator);

		[DllImport(DLL_FILENAME, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void RevolutionSolid_getMeshSize(IntPtr potteryGenerator, out int vertices, out int triangles);

		[DllImport(DLL_FILENAME, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void RevolutionSolid_getMesh(IntPtr potteryGenerator, [Out] UnityEngine.Vector3[] vertices, [Out] UnityEngine.Vector3[] normals, [Out] int[] triangles);

		[DllImport(DLL_FILENAME, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void RevolutionSolid_getMeshEx(IntPtr potteryGenerator, [Out] UnityEngine.Vector3[] vertices, [Out] UnityEngine.Vector3[] normals, [Out] UnityEngine.Vector2[] texCoords, [Out] int[] triangles);

		[DllImport(DLL_FILENAME, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr Voxelizer_create();

		[DllImport(DLL_FILENAME, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Voxelizer_destroy(IntPtr voxelizer);

		[DllImport(DLL_FILENAME, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Voxelizer_voxelize(IntPtr voxelizer, UnityEngine.Vector3[] vertices, int[] triangles, int numVertices, int numTriangles, float voxelSize, VoxelizationFlags voxelizationFlags);

		[DllImport(DLL_FILENAME, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Voxelizer_getVoxelsSize(IntPtr voxelizer, out int vertices, out int triangles);

		[DllImport(DLL_FILENAME, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Voxelizer_getVoxelCentersSize(IntPtr voxelizer, out int vertices);

		[DllImport(DLL_FILENAME, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Voxelizer_getVoxels(IntPtr voxelizer, [Out] UnityEngine.Vector3[] vertices, [Out] UnityEngine.Vector3[] normals, [Out] int[] triangles);

		[DllImport(DLL_FILENAME, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void Voxelizer_getVoxelCenters(IntPtr voxelizer, [Out] UnityEngine.Vector3[] vertices);
	}
}

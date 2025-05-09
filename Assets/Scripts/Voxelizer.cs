using System;

namespace RevolutionSolid
{
	/// <summary>
	/// Flags to pass to the voxelizer when voxelizing a triangle mesh.
	/// </summary>
	/// <remarks>
	/// These flags define what outputs the voxelizer will generate. You can pass either a single
	/// flag, or a combination of these flags to <see cref="Voxelizer.voxelize"/> if you want to
	/// generate multiple outputs.
	/// </remarks>
	/// <seealso cref="Voxelizer.voxelize"/>
	[Flags]
	public enum VoxelizationFlags : uint
	{
		/// <summary>
		/// Generate a triangle mesh that represents the voxel space. This mesh can be used for
		/// visualization.
		/// </summary>
		BuildVoxels = 1,

		/// <summary>
		/// Generate a list of 3D points. Each point is set to the corresponding voxel's center.
		/// The list is used when adding multi-tools to the <see cref="Generator"/> class.
		/// </summary>
		BuildVoxelCenters = 2
	}

	/// <summary>
	/// Class that converts a triangle mesh to a voxel space.
	/// </summary>
	public class Voxelizer
	{
		/// <summary>
		/// Creates an instance of the <see cref="Voxelizer"/> class.
		/// </summary>
		/// <remarks>
		/// <strong>Important:</strong> You should call the <see cref="destroy"/> method when you no
		///   longer need the object (for example, on application quit) so that allocated memory
		///   will be released.
		/// </remarks>
		/// <seealso cref="destroy"/>
		public Voxelizer()
		{
			m_handle = NativeMethods.Voxelizer_create();
		}

		/// <summary>
		/// Destroys the instance.
		/// </summary>
		/// <remarks>
		/// You should call this method when you no longer need the object (for example, on
		///   application quit) so that allocated memory will be released.<br/>
		/// It is safe to call this method multiple times. The first call destroys the instance and
		///   subsequent calls do nothing.<br/>
		/// <strong>Do not call</strong> any other methods of the class (apart from
		///   <see cref="destroy"/>) after you have called <see cref="destroy"/>.
		/// </remarks>
		public void destroy()
		{
			NativeMethods.Voxelizer_destroy(m_handle);

			m_handle = IntPtr.Zero;
		}

		/// <summary>
		/// Voxelize a triangle mesh.
		/// </summary>
		/// <remarks>
		/// You can retrieve the voxelization results by calling <see cref="getVoxels"/> and/or
		/// <see cref="getVoxelCenters"/>.<br/>
		/// Every 3 consecutive indices in the <paramref name="indices"/> array form a triangle.
		/// Thus, the input mesh it is assumed to have <paramref name="indices"/>.Length/3 triangles.
		/// </remarks>
		/// <param name="vertices">Array that contains the vertices of the input mesh.</param>
		/// <param name="indices">Array of triangle indices (3 consecutive indices form a triangle).</param>
		/// <param name="voxelSize">Voxel size. Must be greater than 0. Smaller values produce a
		/// more detailed representation of the input mesh, but the generated voxel space will
		/// require more memory.</param>
		/// <param name="voxelizationFlags">Flags that describe which outputs should be generated.</param>
		/// <seealso cref="getVoxels"/>
		/// <seealso cref="getVoxelCenters"/>
		public void voxelize(UnityEngine.Vector3[] vertices, int[] indices, float voxelSize, VoxelizationFlags voxelizationFlags)
		{
			if (indices.Length % 3 != 0)
			{
				throw new Exception("Invalid array length");
			}

			NativeMethods.Voxelizer_voxelize(m_handle, vertices, indices, vertices.Length, indices.Length / 3, voxelSize, voxelizationFlags);
		}

		/// <summary>
		/// Retrieve a triangle mesh of the voxel space for visualization purposes.
		/// </summary>
		/// <remarks>
		/// The <see cref="Voxelizer.voxelize"/> method should have been called before calling this
		/// method. In addition, the <see cref="VoxelizationFlags.BuildVoxels"/> flag should have
		/// been passed to the <see cref="Voxelizer.voxelize"/> method.<br/>
		/// The lengths of the <paramref name="vertices"/> and <paramref name="normals"/> arrays are
		/// the same. Every 3 consecutive indices in the <paramref name="indices"/> array form a
		/// triangle. Thus, the generated mesh has <paramref name="indices"/>.Length/3 triangles.
		/// </remarks>
		/// <param name="vertices">Array that contains the vertices of the generated mesh.</param>
		/// <param name="normals">Array of unit length normal vectors (one for each vertex).</param>
		/// <param name="indices">Array of triangle indices (3 consecutive indices form a triangle).</param>
		/// <seealso cref="voxelize"/>
		/// <seealso cref="getVoxelCenters"/>
		public void getVoxels(out UnityEngine.Vector3[] vertices, out UnityEngine.Vector3[] normals, out int[] indices)
		{
			int numVertices;
			int numTriangles;

			NativeMethods.Voxelizer_getVoxelsSize(m_handle, out numVertices, out numTriangles);

			vertices = new UnityEngine.Vector3[numVertices];
			normals = new UnityEngine.Vector3[numVertices];
			indices = new int[numTriangles * 3];

			NativeMethods.Voxelizer_getVoxels(m_handle, vertices, normals, indices);
		}

		/// <summary>
		/// Retrieve a list of the voxel centers.
		/// </summary>
		/// <remarks>
		/// The <see cref="Voxelizer.voxelize"/> method should have been called before calling this
		/// method. In addition, the <see cref="VoxelizationFlags.BuildVoxelCenters"/> flag should
		/// have been passed to the <see cref="Voxelizer.voxelize"/> method.
		/// </remarks>
		/// <param name="vertices">Array that contains the voxel centers.</param>
		/// <seealso cref="voxelize"/>
		/// <seealso cref="getVoxels"/>
		public void getVoxelCenters(out UnityEngine.Vector3[] vertices)
		{
			int numVertices;

			NativeMethods.Voxelizer_getVoxelCentersSize(m_handle, out numVertices);

			vertices = new UnityEngine.Vector3[numVertices];

			NativeMethods.Voxelizer_getVoxelCenters(m_handle, vertices);
		}

		IntPtr m_handle = IntPtr.Zero;
	}
}

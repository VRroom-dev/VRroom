using System;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UIElements;

namespace VRroom.Base.Debugging {
	[PublicAPI, UxmlElement]
	public partial class StatisticElement : VisualElement, IDisposable {
		private static readonly int OutputID = Shader.PropertyToID("Output");
		private static readonly int CountID = Shader.PropertyToID("Count");
		private static readonly int MinID = Shader.PropertyToID("Min");
		private static readonly int MaxID = Shader.PropertyToID("Max");
		private static readonly int DataId = Shader.PropertyToID("Data");
		private static readonly int PointsId = Shader.PropertyToID("Points");
		private static readonly int BufferSize = Shader.PropertyToID("BufferSize");
		private static readonly Color TextColor = new(0.7f, 0.7f, 0.7f);
		private ComputeShader _statisticDrawerShader;
		private RenderTexture _statisticDrawerOutput;
		private ComputeBuffer _statisticDrawerPoints;
		private ComputeBuffer _statisticDrawerData;
		private float[] _pointBuffer;
		private NativeArray<Vertex> _vertices;
		private NativeArray<ushort> _indices;
		private StatisticData[] _statisticsData;
		
		public const int PointCount = StatisticTracker.LogWindow * StatisticTracker.LogFPS;
		public float MaxValue = 900;
		public float MinValue;
		
		private struct StatisticData {
			public Color LineColor;
			public uint Head;
			public uint Count;
		}

		private void OnGenerateVisualContent(MeshGenerationContext ctx) {
			if (!Mathf.Approximately(contentRect.width, _statisticDrawerOutput.width) ||
				!Mathf.Approximately(contentRect.height, _statisticDrawerOutput.height)) Resize();
			if (!_statisticDrawerOutput.IsCreated()) return;
			
			StatisticTracker.Statistic[] statistics = StatisticTracker.GetStatistics();
			if (statistics.Length != _statisticsData.Length) {
				Array.Resize(ref _statisticsData, statistics.Length);
				Array.Resize(ref _pointBuffer, statistics.Length * PointCount);
				_statisticDrawerData?.Release();
				_statisticDrawerPoints?.Release();
				_statisticDrawerPoints = new ComputeBuffer(statistics.Length, sizeof(float) * PointCount * statistics.Length);
				_statisticDrawerData = new ComputeBuffer(statistics.Length, Marshal.SizeOf<StatisticData>());
				_statisticDrawerShader.SetInt(CountID, statistics.Length);
				_statisticDrawerShader.SetBuffer(0, DataId, _statisticDrawerData);
				_statisticDrawerShader.SetBuffer(0, PointsId, _statisticDrawerPoints);
			}
			
			for (var i = 0; i < statistics.Length; i++) {
				StatisticTracker.Statistic statistic = statistics[i];
				ref StatisticData data = ref _statisticsData[i];
			
				data.LineColor = statistic.LineColor;
				data.Count = (uint)statistic.Count;
				data.Head = (uint)statistic.Head;
				
				unsafe {
					fixed (float* src = statistic.Points)
					fixed (float* dst = _pointBuffer) {
						float* dstStart = dst + i * PointCount;
						Buffer.MemoryCopy(src, dstStart, sizeof(float) * PointCount, sizeof(float) * PointCount);
					}
				}
			}
			
			_statisticDrawerPoints.SetData(_pointBuffer);
			_statisticDrawerData.SetData(_statisticsData);
			_statisticDrawerShader.SetFloat(MinID, MinValue);
			_statisticDrawerShader.SetFloat(MaxID, MaxValue);
			
			int threadGroupsX = Mathf.CeilToInt(_statisticDrawerOutput.width / 8f);
			int threadGroupsY = Mathf.CeilToInt(_statisticDrawerOutput.height / 8f);
			_statisticDrawerShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);
			
			ctx.DrawMesh(_vertices, _indices, _statisticDrawerOutput);
			
			for (int i = 0; i < 7; i++) {
				float y = contentRect.yMin + contentRect.height / 6 * i;
				float value = Mathf.Lerp(MaxValue, MinValue, i / 6f);
				y -= i switch {
					0 => 2,
					6 => 15,
					_ => 9
				};
			
				ctx.DrawText($"{value:F0}", new(contentRect.xMax + 3, y), 14, TextColor);
			}
		}
	
		public StatisticElement() {
#if UNITY_EDITOR
			if (!UnityEditor.EditorApplication.isPlaying) return;
#endif
			_statisticDrawerShader = Resources.Load<ComputeShader>("StatisticDrawer");
			_statisticDrawerOutput = new(2, 2, 0, GraphicsFormat.R8G8B8A8_UNorm) { enableRandomWrite = true };
			_statisticsData = Array.Empty<StatisticData>();
			_vertices = new(4, Allocator.Persistent);
			_indices = new(6, Allocator.Persistent);

			_vertices[0] = new() {
				position = new(0, 0, Vertex.nearZ),
				uv = new(0, 0),
				tint = Color.white
			};
			_vertices[1] = new() {
				position = new(1, 0, Vertex.nearZ),
				uv = new(1, 0),
				tint = Color.white
			};
			_vertices[2] = new() {
				position = new(1, 1, Vertex.nearZ),
				uv = new(1, 1),
				tint = Color.white
			};
			_vertices[3] = new() {
				position = new(0, 1, Vertex.nearZ),
				uv = new(0, 1),
				tint = Color.white
			};

			_indices[0] = 0;
			_indices[1] = 1;
			_indices[2] = 2;
			_indices[3] = 2;
			_indices[4] = 3;
			_indices[5] = 0;
			
			_statisticDrawerShader.SetInt(BufferSize, PointCount);
			generateVisualContent += OnGenerateVisualContent;
		}

		private void Resize() {
			if (contentRect.width < 16 || contentRect.height < 16) return;
			_statisticDrawerOutput.Release();
			_statisticDrawerOutput = new((int)contentRect.width, (int)contentRect.height, 0) { enableRandomWrite = true };
			_statisticDrawerShader.SetTexture(0, OutputID, _statisticDrawerOutput);

			_vertices[1] = new() {
				position = new(contentRect.width, 0, Vertex.nearZ),
				uv = new(1, 0),
				tint = Color.white
			};
			_vertices[2] = new() {
				position = new(contentRect.width, contentRect.height, Vertex.nearZ),
				uv = new(1, 1),
				tint = Color.white
			};
			_vertices[3] = new() {
				position = new(0, contentRect.height, Vertex.nearZ),
				uv = new(0, 1),
				tint = Color.white
			};
		}

		public void Dispose() {
			if (_statisticDrawerOutput) _statisticDrawerOutput.Release();
			if (_vertices.IsCreated) _vertices.Dispose();
			if (_indices.IsCreated) _indices.Dispose();
			_statisticDrawerData?.Dispose();
		}
	}
}
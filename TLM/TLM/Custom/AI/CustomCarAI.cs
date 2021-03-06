#define DEBUGVx
#define USEPATHWAITCOUNTERx
#define PATHRECALCx

using System;
using System.Collections.Generic;
using ColossalFramework;
using ColossalFramework.Math;
using TrafficManager.Traffic;
using TrafficManager.TrafficLight;
using UnityEngine;
using Random = UnityEngine.Random;
using TrafficManager.Custom.PathFinding;
using TrafficManager.State;

namespace TrafficManager.Custom.AI {
	internal class CustomCarAI : CarAI { // correct would be to inherit from VehicleAI (in order to keep the correct references to `base`)
		public void Awake() {

		}

		internal static void OnLevelUnloading() {

		}

		/// <summary>
		/// Lightweight simulation step method.
		/// This method is occasionally being called for different cars.
		/// </summary>
		/// <param name="vehicleId"></param>
		/// <param name="vehicleData"></param>
		/// <param name="physicsLodRefPos"></param>
		public void CustomSimulationStep(ushort vehicleId, ref Vehicle vehicleData, Vector3 physicsLodRefPos) {
#if USEPATHWAITCOUNTER
			VehicleState state = VehicleStateManager._GetVehicleState(vehicleId);
#endif

			if ((vehicleData.m_flags & Vehicle.Flags.WaitingPath) != 0) {
				PathManager instance = Singleton<PathManager>.instance;
				byte pathFindFlags = instance.m_pathUnits.m_buffer[(int)((UIntPtr)vehicleData.m_path)].m_pathFindFlags;
				if ((pathFindFlags & PathUnit.FLAG_READY) != 0) {
#if USEPATHWAITCOUNTER
					state.PathWaitCounter = 0; // NON-STOCK CODE
#endif
					vehicleData.m_pathPositionIndex = 255;
					vehicleData.m_flags &= ~Vehicle.Flags.WaitingPath;
					vehicleData.m_flags &= ~Vehicle.Flags.Arriving;
					this.PathfindSuccess(vehicleId, ref vehicleData);
					this.TrySpawn(vehicleId, ref vehicleData);
					VehicleStateManager.OnPathFindReady(vehicleId, ref vehicleData); // NON-STOCK CODE
				} else if ((pathFindFlags & PathUnit.FLAG_FAILED) != 0 || vehicleData.m_path == 0
#if USEPATHWAITCOUNTER
					|| ((pathFindFlags & PathUnit.FLAG_CREATED) != 0 && state.PathWaitCounter == ushort.MaxValue)
#endif
					) { // NON-STOCK CODE
#if USEPATHWAITCOUNTER
					state.PathWaitCounter = 0; // NON-STOCK CODE
#endif
					vehicleData.m_flags &= ~Vehicle.Flags.WaitingPath;
					Singleton<PathManager>.instance.ReleasePath(vehicleData.m_path);
					vehicleData.m_path = 0u;
					this.PathfindFailure(vehicleId, ref vehicleData);
					return;
				}
#if USEPATHWAITCOUNTER
				else {
					state.PathWaitCounter = (ushort)Math.Min(ushort.MaxValue, (int)state.PathWaitCounter+1); // NON-STOCK CODE
				}
#endif
			} else {
#if USEPATHWAITCOUNTER
				state.PathWaitCounter = 0; // NON-STOCK CODE
#endif
				if ((vehicleData.m_flags & Vehicle.Flags.WaitingSpace) != 0) {
					this.TrySpawn(vehicleId, ref vehicleData);
				}
			}

			try {
				VehicleStateManager.LogTraffic(vehicleId, ref vehicleData, true);
			} catch (Exception e) {
				Log.Error("CarAI CustomSimulationStep Error: " + e.ToString());
			}

			try {
				VehicleStateManager.UpdateVehiclePos(vehicleId, ref vehicleData);
			} catch (Exception e) {
				Log.Error("CarAI CustomSimulationStep Error: " + e.ToString());
			}

			Vector3 lastFramePosition = vehicleData.GetLastFramePosition();
			int lodPhysics;
			if (Vector3.SqrMagnitude(physicsLodRefPos - lastFramePosition) >= 1210000f) {
				lodPhysics = 2;
			} else if (Vector3.SqrMagnitude(Singleton<SimulationManager>.instance.m_simulationView.m_position - lastFramePosition) >= 250000f) {
				lodPhysics = 1;
			} else {
				lodPhysics = 0;
			}
			this.SimulationStep(vehicleId, ref vehicleData, vehicleId, ref vehicleData, lodPhysics);
			if (vehicleData.m_leadingVehicle == 0 && vehicleData.m_trailingVehicle != 0) {
				VehicleManager instance2 = Singleton<VehicleManager>.instance;
				ushort num = vehicleData.m_trailingVehicle;
				int num2 = 0;
				while (num != 0) {
					ushort trailingVehicle = instance2.m_vehicles.m_buffer[(int)num].m_trailingVehicle;
					VehicleInfo info = instance2.m_vehicles.m_buffer[(int)num].Info;
					info.m_vehicleAI.SimulationStep(num, ref instance2.m_vehicles.m_buffer[(int)num], vehicleId, ref vehicleData, lodPhysics);
					num = trailingVehicle;
					if (++num2 > 16384) {
						CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
						break;
					}
				}
			}
#if PATHRECALC
			ushort recalcSegmentId = 0;
#endif
			int privateServiceIndex = ItemClass.GetPrivateServiceIndex(this.m_info.m_class.m_service);
			int maxBlockCounter = (privateServiceIndex == -1) ? 150 : 100;
			if ((vehicleData.m_flags & (Vehicle.Flags.Spawned | Vehicle.Flags.WaitingPath | Vehicle.Flags.WaitingSpace)) == 0 && vehicleData.m_cargoParent == 0) {
				Singleton<VehicleManager>.instance.ReleaseVehicle(vehicleId);
			} else if ((int)vehicleData.m_blockCounter == maxBlockCounter && Options.enableDespawning) {
				Singleton<VehicleManager>.instance.ReleaseVehicle(vehicleId);
			}
#if PATHRECALC
			else if (vehicleData.m_leadingVehicle == 0 && CustomVehicleAI.ShouldRecalculatePath(vehicleId, ref vehicleData, maxBlockCounter, out recalcSegmentId)) {
				CustomVehicleAI.MarkPathRecalculation(vehicleId, recalcSegmentId);
				InvalidPath(vehicleId, ref vehicleData, vehicleId, ref vehicleData);
			}
#endif
		}

		public void CustomCalculateSegmentPosition(ushort vehicleId, ref Vehicle vehicleData, PathUnit.Position nextPosition,
				PathUnit.Position position, uint laneID, byte offset, PathUnit.Position prevPos, uint prevLaneID,
				byte prevOffset, int index, out Vector3 pos, out Vector3 dir, out float maxSpeed) {
			if (Options.simAccuracy <= 1) {
				try {
					VehicleStateManager.UpdateVehiclePos(vehicleId, ref vehicleData, ref prevPos, ref position);
				} catch (Exception e) {
					Log.Error("CarAI CustomCalculateSegmentPosition Error: " + e.ToString());
				}
			}

			var netManager = Singleton<NetManager>.instance;
			//var vehicleManager = Singleton<VehicleManager>.instance;
			netManager.m_lanes.m_buffer[(int)((UIntPtr)laneID)].CalculatePositionAndDirection(offset * 0.003921569f, out pos, out dir);

			Vehicle.Frame lastFrameData = vehicleData.GetLastFrameData();
			Vector3 lastFrameVehiclePos = lastFrameData.m_position;

			var camPos = Camera.main.transform.position;

#if DEBUG
			//bool isEmergency = VehicleStateManager._GetVehicleState(vehicleId).VehicleType == ExtVehicleType.Emergency;
#endif

			// I think this is supposed to be the lane position?
			// [VN, 12/23/2015] It's the 3D car position on the Bezier curve of the lane.
			// This crazy 0.003921569f equals to 1f/255 and prevOffset is the byte value (0..255) of the car position.
			var vehiclePosOnBezier = netManager.m_lanes.m_buffer[(int)((UIntPtr)prevLaneID)].CalculatePosition(prevOffset * 0.003921569f);
			//ushort currentSegmentId = netManager.m_lanes.m_buffer[(int)((UIntPtr)prevLaneID)].m_segment;

			ushort targetNodeId;
			ushort nextTargetNodeId;
			if (offset < position.m_offset) {
				targetNodeId = netManager.m_segments.m_buffer[position.m_segment].m_startNode;
				nextTargetNodeId = netManager.m_segments.m_buffer[position.m_segment].m_endNode;
			} else {
				targetNodeId = netManager.m_segments.m_buffer[position.m_segment].m_endNode;
				nextTargetNodeId = netManager.m_segments.m_buffer[position.m_segment].m_startNode;
			}
			var prevTargetNodeId = prevOffset == 0 ? netManager.m_segments.m_buffer[prevPos.m_segment].m_startNode : netManager.m_segments.m_buffer[prevPos.m_segment].m_endNode;

			// this seems to be like the required braking force in order to stop the vehicle within its half length.
			var crazyValue = 0.5f * lastFrameData.m_velocity.sqrMagnitude / m_info.m_braking + m_info.m_generatedInfo.m_size.z * 0.5f;

			/*try {
				VehicleStateManager.UpdateVehiclePos(vehicleId, ref vehicleData);
			} catch (Exception e) {
				Log.Error("CarAI TmCalculateSegmentPosition Error: " + e.ToString());
			}*/

			bool isRecklessDriver = IsRecklessDriver(vehicleId, ref vehicleData);
			if (targetNodeId == prevTargetNodeId) {
				if (Vector3.Distance(lastFrameVehiclePos, vehiclePosOnBezier) >= crazyValue - 1f) {
					if (!CustomVehicleAI.MayChangeSegment(vehicleId, ref vehicleData, ref lastFrameData, isRecklessDriver, ref prevPos, prevTargetNodeId, prevLaneID, ref position, targetNodeId, laneID, ref nextPosition, nextTargetNodeId, out maxSpeed))
						return;
				}
			}

			var info2 = netManager.m_segments.m_buffer[position.m_segment].Info;
			if (info2.m_lanes != null && info2.m_lanes.Length > position.m_lane) {
				var laneSpeedLimit = SpeedLimitManager.GetLockFreeGameSpeedLimit(position.m_segment, position.m_lane, laneID, info2.m_lanes[position.m_lane]); // info2.m_lanes[position.m_lane].m_speedLimit;

#if DEBUG
				/*if (position.m_segment == 275) {
					Log._Debug($"Applying lane speed limit of {laneSpeedLimit} to lane {laneID} @ seg. {position.m_segment}");
                }*/
#endif

				/*if (TrafficRoadRestrictions.IsSegment(position.m_segment)) {
					var restrictionSegment = TrafficRoadRestrictions.GetSegment(position.m_segment);

					if (restrictionSegment.SpeedLimits[position.m_lane] > 0.1f) {
						laneSpeedLimit = restrictionSegment.SpeedLimits[position.m_lane];
					}
				}*/

				maxSpeed = CalculateTargetSpeed(vehicleId, ref vehicleData, laneSpeedLimit, netManager.m_lanes.m_buffer[(int)((UIntPtr)laneID)].m_curve);
			} else {
				maxSpeed = CalculateTargetSpeed(vehicleId, ref vehicleData, 1f, 0f);
			}

			maxSpeed = CalcMaxSpeed(vehicleId, ref vehicleData, position, pos, maxSpeed, isRecklessDriver);
		}

		

		internal static readonly float MIN_SPEED = 8f * 0.2f; // 10 km/h
		internal static readonly float ICY_ROADS_MIN_SPEED = 8f * 0.4f; // 20 km/h
		internal static readonly float ICY_ROADS_STUDDED_MIN_SPEED = 8f * 0.8f; // 40 km/h
		internal static readonly float WET_ROADS_MAX_SPEED = 8f * 1.6f; // 80 km/h
		internal static readonly float WET_ROADS_FACTOR = 0.75f;
		internal static readonly float BROKEN_ROADS_MAX_SPEED = 8f * 1.6f; // 80 km/h
		internal static readonly float BROKEN_ROADS_FACTOR = 0.75f;

		internal static float CalcMaxSpeed(ushort vehicleId, ref Vehicle vehicleData, PathUnit.Position position, Vector3 pos, float maxSpeed, bool isRecklessDriver) {
			var netManager = Singleton<NetManager>.instance;
			NetInfo segmentInfo = netManager.m_segments.m_buffer[(int)position.m_segment].Info;
			bool highwayRules = (segmentInfo.m_netAI is RoadBaseAI && ((RoadBaseAI)segmentInfo.m_netAI).m_highwayRules);

			if (!highwayRules) {
				if (netManager.m_treatWetAsSnow) {
					DistrictManager districtManager = Singleton<DistrictManager>.instance;
					byte district = districtManager.GetDistrict(pos);
					DistrictPolicies.CityPlanning cityPlanningPolicies = districtManager.m_districts.m_buffer[(int)district].m_cityPlanningPolicies;
					if ((cityPlanningPolicies & DistrictPolicies.CityPlanning.StuddedTires) != DistrictPolicies.CityPlanning.None) {
						if (Options.strongerRoadConditionEffects) {
							if (maxSpeed > ICY_ROADS_STUDDED_MIN_SPEED)
								maxSpeed = ICY_ROADS_STUDDED_MIN_SPEED + (float)(255 - netManager.m_segments.m_buffer[(int)position.m_segment].m_wetness) * 0.0039215686f * (maxSpeed - ICY_ROADS_STUDDED_MIN_SPEED);
						} else {
							maxSpeed *= 1f - (float)netManager.m_segments.m_buffer[(int)position.m_segment].m_wetness * 0.0005882353f; // vanilla: -15% .. �0%
						}
						districtManager.m_districts.m_buffer[(int)district].m_cityPlanningPoliciesEffect |= DistrictPolicies.CityPlanning.StuddedTires;
					} else {
						if (Options.strongerRoadConditionEffects) {
							if (maxSpeed > ICY_ROADS_MIN_SPEED)
								maxSpeed = ICY_ROADS_MIN_SPEED + (float)(255 - netManager.m_segments.m_buffer[(int)position.m_segment].m_wetness) * 0.0039215686f * (maxSpeed - ICY_ROADS_MIN_SPEED);
						} else {
							maxSpeed *= 1f - (float)netManager.m_segments.m_buffer[(int)position.m_segment].m_wetness * 0.00117647066f; // vanilla: -30% .. �0%
						}
					}
				} else {
					if (Options.strongerRoadConditionEffects) {
						float minSpeed = Math.Min(maxSpeed * WET_ROADS_FACTOR, WET_ROADS_MAX_SPEED);
						if (maxSpeed > minSpeed)
							maxSpeed = minSpeed + (float)(255 - netManager.m_segments.m_buffer[(int)position.m_segment].m_wetness) * 0.0039215686f * (maxSpeed - minSpeed);
					} else {
						maxSpeed *= 1f - (float)netManager.m_segments.m_buffer[(int)position.m_segment].m_wetness * 0.0005882353f; // vanilla: -15% .. �0%
					}
				}

				if (Options.strongerRoadConditionEffects) {
					float minSpeed = Math.Min(maxSpeed * BROKEN_ROADS_FACTOR, BROKEN_ROADS_MAX_SPEED);
					if (maxSpeed > minSpeed) {
						maxSpeed = minSpeed + (float)netManager.m_segments.m_buffer[(int)position.m_segment].m_condition * 0.0039215686f * (maxSpeed - minSpeed);
					}
				} else {
					maxSpeed *= 1f + (float)netManager.m_segments.m_buffer[(int)position.m_segment].m_condition * 0.0005882353f; // vanilla: �0% .. +15 %
				}
			}

			//ExtVehicleType? vehicleType = VehicleStateManager.GetVehicleState(vehicleId)?.VehicleType;
			float vehicleRand = Math.Min(1f, (float)(vehicleId % 101) * 0.01f); // we choose 101 because it's a prime number
			if (isRecklessDriver)
				maxSpeed *= 1.5f + vehicleRand * 0.5f; // woohooo, 1.5 .. 2
			else
				maxSpeed *= 0.7f + vehicleRand * 0.6f; // a little variance, 0.7 .. 1.3
			/*else if ((vehicleType & ExtVehicleType.PassengerCar) != ExtVehicleType.None)
				maxSpeed *= 0.7f + vehicleRand * 0.4f; // a little variance, 0.7 .. 1.1
			else if ((vehicleType & ExtVehicleType.Taxi) != ExtVehicleType.None)
				maxSpeed *= 0.9f + vehicleRand * 0.4f; // a little variance, 0.9 .. 1.3*/

			maxSpeed = Math.Max(MIN_SPEED, maxSpeed); // at least 10 km/h

			return maxSpeed;
		}

		public void CustomCalculateSegmentPositionPathFinder(ushort vehicleId, ref Vehicle vehicleData, PathUnit.Position position, uint laneId, byte offset, out Vector3 pos, out Vector3 dir, out float maxSpeed) {
			var netManager = Singleton<NetManager>.instance;
			netManager.m_lanes.m_buffer[(int)((UIntPtr)laneId)].CalculatePositionAndDirection(offset * 0.003921569f,
				out pos, out dir);
			var info = netManager.m_segments.m_buffer[position.m_segment].Info;
			if (info.m_lanes != null && info.m_lanes.Length > position.m_lane) {
				var laneSpeedLimit = SpeedLimitManager.GetLockFreeGameSpeedLimit(position.m_segment, position.m_lane, laneId, info.m_lanes[position.m_lane]); //info.m_lanes[position.m_lane].m_speedLimit;
				maxSpeed = CalculateTargetSpeed(vehicleId, ref vehicleData, laneSpeedLimit, netManager.m_lanes.m_buffer[(int)((UIntPtr)laneId)].m_curve);
			} else {
				maxSpeed = CalculateTargetSpeed(vehicleId, ref vehicleData, 1f, 0f);
			}

			maxSpeed = CalcMaxSpeed(vehicleId, ref vehicleData, position, pos, maxSpeed, IsRecklessDriver(vehicleId, ref vehicleData));
		}

		internal static bool IsRecklessDriver(ushort vehicleId, ref Vehicle vehicleData) {
			if ((vehicleData.m_flags & Vehicle.Flags.Emergency2) != 0)
				return true;
			if (Options.recklessDrivers == 3)
				return false;

			return ((vehicleData.Info.m_vehicleType & VehicleInfo.VehicleType.Car) != VehicleInfo.VehicleType.None) && (uint)vehicleId % (Options.getRecklessDriverModulo()) == 0;
		}

		public bool CustomStartPathFind(ushort vehicleID, ref Vehicle vehicleData, Vector3 startPos, Vector3 endPos, bool startBothWays, bool endBothWays, bool undergroundTarget) {
			ExtVehicleType? vehicleType = VehicleStateManager.DetermineVehicleType(vehicleID, ref vehicleData);
#if PATHRECALC
			VehicleState state = VehicleStateManager._GetVehicleState(vehicleID);
			bool recalcRequested = state.PathRecalculationRequested;
			state.PathRecalculationRequested = false;
#endif
			/*if (vehicleType == null) {
				Log._Debug($"CustomCarAI.CustomStartPathFind: Could not determine ExtVehicleType from class type. typeof this={this.GetType().ToString()}");
			} else {
				Log._Debug($"CustomCarAI.CustomStartPathFind: vehicleType={vehicleType}. typeof this={this.GetType().ToString()}");
			}*/

			VehicleInfo info = this.m_info;
			bool allowUnderground = (vehicleData.m_flags & (Vehicle.Flags.Underground | Vehicle.Flags.Transition)) != 0;
			PathUnit.Position startPosA;
			PathUnit.Position startPosB;
			float num;
			float num2;
			PathUnit.Position endPosA;
			PathUnit.Position endPosB;
			float num3;
			float num4;
			if (CustomPathManager.FindPathPosition(startPos, ItemClass.Service.Road, NetInfo.LaneType.Vehicle | NetInfo.LaneType.TransportVehicle, info.m_vehicleType, allowUnderground, false, 32f, out startPosA, out startPosB, out num, out num2) &&
				CustomPathManager.FindPathPosition(endPos, ItemClass.Service.Road, NetInfo.LaneType.Vehicle | NetInfo.LaneType.TransportVehicle, info.m_vehicleType, undergroundTarget, false, 32f, out endPosA, out endPosB, out num3, out num4)) {
				if (!startBothWays || num < 10f) {
					startPosB = default(PathUnit.Position);
				}
				if (!endBothWays || num3 < 10f) {
					endPosB = default(PathUnit.Position);
				}
				uint path;
				bool res = false;
				if (vehicleType == null)
					res = Singleton<CustomPathManager>.instance.CreatePath(out path, ref Singleton<SimulationManager>.instance.m_randomizer, Singleton<SimulationManager>.instance.m_currentBuildIndex, startPosA, startPosB, endPosA, endPosB, NetInfo.LaneType.Vehicle, info.m_vehicleType, 20000f, this.IsHeavyVehicle(), this.IgnoreBlocked(vehicleID, ref vehicleData), false, false);
				else
					res = Singleton<CustomPathManager>.instance.CreatePath(
#if PATHRECALC
						recalcRequested, 
#endif
						(ExtVehicleType)vehicleType, out path, ref Singleton<SimulationManager>.instance.m_randomizer, Singleton<SimulationManager>.instance.m_currentBuildIndex, ref startPosA, ref startPosB, ref endPosA, ref endPosB, NetInfo.LaneType.Vehicle, info.m_vehicleType, 20000f, this.IsHeavyVehicle(), this.IgnoreBlocked(vehicleID, ref vehicleData), false, false);
				if (res) {
					if (vehicleData.m_path != 0u) {
						Singleton<PathManager>.instance.ReleasePath(vehicleData.m_path);
					}
					vehicleData.m_path = path;
					vehicleData.m_flags |= Vehicle.Flags.WaitingPath;
					return true;
				}
			}
			return false;
		}
	}
}
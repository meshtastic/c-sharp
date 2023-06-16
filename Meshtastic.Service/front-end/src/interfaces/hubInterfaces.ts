import { INode } from "../services/meshtastic.api.clients"
export interface IFromRadioViewModel {
  node: INode,
  port: PortNum,
  payload: String,
  timestamp: Date
}

export enum PortNum {
  UnknownApp = 0,
  TextMessageApp = 1,
  RemoteHardwareApp = 2,
  PositionApp = 3,
  NodeinfoApp = 4,
  RoutingApp = 5,
  AdminApp = 6,
  TextMessageCompressedApp = 7,
  WaypointApp = 8,
  AudioApp = 9,
  ReplyApp = 32,
  IpTunnelApp = 33,
  SerialApp = 64,
  StoreForwardApp = 65,
  RangeTestApp = 66,
  TelemetryApp = 67,
  ZpsApp = 68,
  SimulatorApp = 69,
  TracerouteApp = 70,
  PrivateApp = 256,
  AtakForwarder = 257,
  Max = 511,
}
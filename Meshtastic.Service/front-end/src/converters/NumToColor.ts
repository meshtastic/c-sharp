import { PortNum } from "../interfaces/hubInterfaces"

export function numToColor(num: number): string {
  var red = (num & 0xFF0000) >> 16
  var green = (num & 0x00FF00) >> 8
  var blue = num & 0x0000FF
  return `rgb(${red}, ${green}, ${blue})`
}
export function portNumToColor(port: PortNum | number): string {
  if (port == PortNum.TextMessageApp) 
    return '#FF0000'
  else if (port == PortNum.PositionApp) 
    return '#00FF00'
  else if (port == PortNum.TelemetryApp) 
    return '#0000FF'
  else if (port == PortNum.WaypointApp) 
    return '#00FFFF'
  return '#FFFFFF'
} 


export function isLightColor(num: number): boolean {
  var red = (num & 0xFF0000) >> 16
  var green = (num & 0x00FF00) >> 8
  var blue = num & 0x0000FF
  
  const brightness = (0.299 * red + 0.587 * green + 0.114 * blue)/255;
  return (brightness > 0.5)
} 
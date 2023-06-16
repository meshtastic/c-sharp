
<template>
  <div class="container mx-auto rounded-xl shadow border p-8 ">
    <div v-for="packet in packets" class="py-3">
      <node-badge :node="packet.node" :id="packet.node.id" />
      <span class="text-sm py-1 float-right">
        {{ new Date(packet.timestamp).toLocaleString('en-US', { hour12: true }) }}
      </span>
      <span class="inline-flex items-center rounded-md bg-gray-50 px-2 py-1 text-xs ring-1 ring-inset ring-gray-500/10 mx-2 float-right"
        :class="{ 'text-black': isLightColor(packet.port), 'text-white': !isLightColor(packet.port) }"
        :style="{ background: portNumToColor(packet.port) }">
        {{ PortNum[packet.port] }}
      </span>
      <div class="font-mono pt-1">
        {{ packet.payload }}
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
  import { ref } from "vue"
  import * as signalR from "@microsoft/signalr"
  import { useDashboardStore } from '../stores/dashboardStore.js'
  import { IFromRadioViewModel, PortNum } from '../interfaces/hubInterfaces'
  import { portNumToColor, isLightColor } from '../converters/NumToColor'
  import NodeBadge from "./Nodes/NodeBadge.vue"
  const dashboardStore = useDashboardStore()

  const packets = ref(new Array<IFromRadioViewModel>)
  const hubConnection = new signalR.HubConnectionBuilder()
    .withUrl("/broker")
    .build()

  hubConnection.start().then(() => {
    hubConnection.on('FromRadioReceived', fromRadio => {
      packets.value.push(fromRadio)
      dashboardStore.getDashboard()
    })
  })
  
  
</script>

<template>
  <!--Container-->
  <div class="container w-full mx-auto pt-20">
    <div class="w-full px-4 md:px-0 md:mt-8 mb-16 leading-normal">
      <h1 class="mb-4 text-4xl text-center font-bold font-sans">
        Meshtastic Service
      </h1>
    </div>
    <!-- {{ dashboadStore.dashboard?.nodes }} -->
    <hr class="border-b-2 border-gray-600 my-8 mx-4">
    <div class="mt-2 mx-4">
      <nodes-table />
    </div>
    <div class="flex flex-row flex-wrap flex-grow mt-2">
      <traffic-type-chart />
      <traffic-nodes-chart />
    </div>
    <div class="flex flex-row flex-wrap flex-grow mt-2">
      <log-panel />
    </div>
  </div>
</template>

<script setup lang="ts">
import TrafficTypeChart from './components/TrafficTypeChart.vue'
import TrafficNodesChart from './components/TrafficNodesChart.vue'
import LogPanel from './components/LogPanel.vue'
import NodesTable from './components/Nodes/NodesTable.vue'

import { useDashboardStore } from './stores/dashboardStore'
import { onMounted } from 'vue'
const dashboadStore = useDashboardStore()
onMounted(() => {
  dashboadStore.getDashboard()
})

</script>

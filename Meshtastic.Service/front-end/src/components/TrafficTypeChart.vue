<template>
  <div class="w-full md:w-1/2 p-3">
    <!--Graph Card-->
    <div class="bg-gray-900 border border-gray-800 rounded shadow">
      <div class="border-b border-gray-800 p-3">
        <h5 class="font-bold uppercase text-gray-200">
          Traffic by App
        </h5>
      </div>
      <div class="p-5">
        <DoughnutChart :chartData="trafficData" />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useDashboardStore } from '../stores/dashboardStore.js'
import { DoughnutChart } from 'vue-chart-3'
import { Chart, ChartData, registerables } from 'chart.js'
import { ref,  computed } from 'vue'
import { portNumToColor } from '../converters/NumToColor'

Chart.register(...registerables)
const dashboardStore = useDashboardStore()
const traffic = ref(dashboardStore.dashboard?.trafficByPort || [])

const trafficData = computed<ChartData<"doughnut">>(() => ({
  labels: traffic.value.map(t => t.name),
  datasets: [
    {
      data: traffic.value.map(t => t.count),
      backgroundColor: traffic.value.map(t => portNumToColor(t.id)),
    },
  ]
}))
</script>
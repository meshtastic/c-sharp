<template>
  <div class="relative overflow-x-auto shadow-md sm:rounded-lg">
    <table class="w-full text-sm text-left text-gray-500 dark:text-gray-400">
      <thead class="text-xs text-gray-700 uppercase bg-gray-50 dark:bg-gray-700 dark:text-gray-400">
        <tr>
          <th scope="col" class="px-6 py-3">
            Name
          </th>
          <th scope="col" class="px-6 py-3">
            ID
          </th>
          <th scope="col" class="px-6 py-3">
            Battery Level
          </th>
          <th scope="col" class="px-6 py-3">
            Position
          </th>
          <th scope="col" class="px-6 py-3">
            Airtime Util.
          </th>
          <th scope="col" class="px-6 py-3">
            Channel Util.
          </th>
          <th scope="col" class="px-6 py-3">
            Last Heard
          </th>
          <th scope="col" class="px-6 py-3">
            
          </th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="node in nodes" :key="node.id" class="bg-white border-b dark:bg-gray-900 dark:border-gray-700">
          <th scope="row" class="px-6 py-4 font-medium text-gray-900 whitespace-nowrap dark:text-white">
            <node-badge :node="node" :id="node.id" />
          </th>
          <td class="px-6 py-4">
            {{ node.id }}
          </td>
          <td class="px-6 py-4">
            {{ node.lastBatteryLevel }}%
          </td>
          <td class="px-6 py-4">
            <span v-if="node.lastLatitude != null && node.lastLatitude != 0">
              {{ node.lastLatitude.toFixed(7) }}, {{ node.lastLongitude!.toFixed(7) }}
            </span>
          </td>
          <td class="px-6 py-4">
            <span v-if="node.lastAirUtilTx">
              {{ node.lastAirUtilTx.toFixed(2) }}%
            </span>
          </td>
          <td class="px-6 py-4">
            <span v-if="node.lastChannelUtilTx">
              {{ node.lastChannelUtilTx.toFixed(2) }}%
            </span>
          </td>
          <td class="px-6 py-4">
            {{ node.lastHeardFrom.toLocaleString('en-US', { hour12: true }) }}
          </td>
          <td class="px-6 py-4">
            <a href="#" class="font-medium text-blue-600 dark:text-blue-500 hover:underline">Edit</a>
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<script setup lang="ts">
import NodeBadge from './NodeBadge.vue'
import { useDashboardStore } from '../../stores/dashboardStore.js'
import { computed } from 'vue'

const dashboadStore = useDashboardStore()

const nodes = computed(() => dashboadStore.dashboard?.nodes || [])
</script>
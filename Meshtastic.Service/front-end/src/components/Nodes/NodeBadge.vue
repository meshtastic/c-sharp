<template>
  <span class="inline-flex items-center rounded-md bg-gray-50 px-2 py-1 text-xs font-medium ring-1 ring-inset ring-gray-500/10"
    :class="{ 'text-black': isLightColor(id), 'text-white': !isLightColor(id) }"
    :style="{ background: numToColor(id) }">
    {{ fullName }}
  </span>
</template>

<script setup lang="ts">
import { numToColor, isLightColor } from '../../converters/NumToColor'
import { computed } from 'vue'
import { INode } from '../../services/meshtastic.api.clients'

const props = defineProps<{
  node?: INode,
  id: number
}>()

const id = computed(() => props.id)
const fullName = computed(() => props.node ? `${props.node.name} (${props.node.shortName})` : `Unknown`)

</script>
import { defineStore } from 'pinia'
import { DashboardService, DashboardViewModel } from '../services/meshtastic.api.clients'

const dashboardService = new DashboardService()

export const useDashboardStore = defineStore('dashboardStore', {
  state: () => ({
    dashboard: undefined as DashboardViewModel | undefined,
  }),

  actions: {
    async getDashboard() {
      try {
        this.dashboard = await dashboardService.get()
      }
      catch (error) {
        return error
      }
    }
  }
})
<script setup>
import { computed, onMounted, ref } from 'vue'
import { api } from './api'

const statuses = ['Open', 'InProgress', 'Completed', 'OnHold']
const priorities = ['Low', 'Medium', 'High', 'Urgent']

const workOrders = ref([])
const selectedStatusFilter = ref('')
const selectedWorkOrder = ref(null)
const loading = ref(false)
const errorMessage = ref('')
const form = ref({
  title: '',
  status: 'Open',
  priority: 'Medium',
  assignedTo: '',
  dueDate: '',
})
const statusUpdate = ref({
  toStatus: 'InProgress',
  changedBy: 'Dispatcher',
})

const selectedStatusLabel = computed(() => {
  return selectedStatusFilter.value ? `Status: ${selectedStatusFilter.value}` : 'All statuses'
})

async function loadWorkOrders() {
  loading.value = true
  errorMessage.value = ''

  try {
    workOrders.value = await api.getWorkOrders(selectedStatusFilter.value)
  } catch (error) {
    errorMessage.value = error.message
  } finally {
    loading.value = false
  }
}

async function openWorkOrder(id) {
  errorMessage.value = ''

  try {
    selectedWorkOrder.value = await api.getWorkOrder(id)
  } catch (error) {
    errorMessage.value = error.message
  }
}

async function createWorkOrder() {
  if (!form.value.title.trim() || !form.value.assignedTo.trim() || !form.value.dueDate) {
    errorMessage.value = 'Title, assignee, and due date are required.'
    return
  }

  try {
    await api.createWorkOrder({
      title: form.value.title,
      status: form.value.status,
      priority: form.value.priority,
      assignedTo: form.value.assignedTo,
      dueDate: new Date(`${form.value.dueDate}T00:00:00Z`).toISOString(),
    })

    form.value = {
      title: '',
      status: 'Open',
      priority: 'Medium',
      assignedTo: '',
      dueDate: '',
    }

    await loadWorkOrders()
    errorMessage.value = ''
  } catch (error) {
    errorMessage.value = error.message
  }
}

async function updateStatus() {
  if (!selectedWorkOrder.value) {
    return
  }

  try {
    const now = new Date().toISOString()
    const current = selectedWorkOrder.value

    await api.updateStatus(current.id, {
      workOrderId: current.id,
      fromStatus: current.status,
      toStatus: statusUpdate.value.toStatus,
      changedAt: now,
      changedBy: statusUpdate.value.changedBy || 'Dispatcher',
      workOrder: {
        id: current.id,
        title: current.title,
        status: current.status,
        priority: current.priority,
        assignedTo: current.assignedTo,
        dueDate: current.dueDate,
        statusChanges: current.statusChanges || [],
      },
    })

    await openWorkOrder(current.id)
    await loadWorkOrders()
    errorMessage.value = ''
  } catch (error) {
    errorMessage.value = error.message
  }
}

function formatDate(value) {
  if (!value) return '—'
  return new Date(value).toLocaleDateString(undefined, {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  })
}

function formatDateTime(value) {
  if (!value) return '—'
  return new Date(value).toLocaleString(undefined, {
    dateStyle: 'medium',
    timeStyle: 'short',
  })
}

onMounted(() => {
  loadWorkOrders()
})
</script>

<template>
  <div class="page-shell">
    <header class="topbar">
      <div>
        <p class="eyebrow">Dispatcher workflows</p>
        <h1>Work Orders</h1>
      </div>
    </header>

    <div v-if="errorMessage" class="banner error">{{ errorMessage }}</div>

    <main class="layout">
      <section class="panel list-panel">
        <div class="panel-header">
          <h2>Queue</h2>
          <div class="toolbar">
            <select v-model="selectedStatusFilter" @change="loadWorkOrders">
              <option value="">All statuses</option>
              <option v-for="status in statuses" :key="status" :value="status">
                {{ status }}
              </option>
            </select>
          </div>
        </div>

        <div v-if="loading" class="empty-state">Loading work orders...</div>
        <ul v-else class="work-order-list">
          <li
            v-for="item in workOrders"
            :key="item.id"
            :class="['work-order-item', { active: selectedWorkOrder && selectedWorkOrder.id === item.id }]"
            @click="openWorkOrder(item.id)"
          >
            <div class="meta-row">
              <span class="status-pill" :class="item.status">{{ item.status }}</span>
              <span class="priority-pill" :class="item.priority">{{ item.priority }}</span>
            </div>
            <strong>{{ item.title }}</strong>
            <div class="line-items">
              <span>Assigned: {{ item.assignedTo || 'Unassigned' }}</span>
              <span>Due: {{ formatDate(item.dueDate) }}</span>
            </div>
          </li>
        </ul>
      </section>

      <section class="panel details-panel">
        <div v-if="selectedWorkOrder" class="detail-card">
          <div class="panel-header detail-header">
            <div>
              <p class="eyebrow">Selected work order</p>
              <h2>{{ selectedWorkOrder.title }}</h2>
            </div>
            <div class="status-row">
              <span class="status-pill" :class="selectedWorkOrder.status">{{ selectedWorkOrder.status }}</span>
            </div>
          </div>

          <div class="detail-grid">
            <div>
              <label>Priority</label>
              <div>{{ selectedWorkOrder.priority }}</div>
            </div>
            <div>
              <label>Assigned to</label>
              <div>{{ selectedWorkOrder.assignedTo || 'Unassigned' }}</div>
            </div>
            <div>
              <label>Due date</label>
              <div>{{ formatDate(selectedWorkOrder.dueDate) }}</div>
            </div>
          </div>

          <div class="status-update-box">
            <h3>Update status</h3>
            <div class="inline-form">
              <select v-model="statusUpdate.toStatus">
                <option v-for="status in statuses" :key="status" :value="status">
                  {{ status }}
                </option>
              </select>
              <input v-model="statusUpdate.changedBy" type="text" placeholder="Changed by" />
              <button @click="updateStatus">Save</button>
            </div>
          </div>

          <div class="history-box">
            <h3>Recent status changes</h3>
            <ul v-if="selectedWorkOrder.statusChanges?.length" class="history-list">
              <li v-for="change in selectedWorkOrder.statusChanges" :key="change.id">
                <strong>{{ change.fromStatus }} → {{ change.toStatus }}</strong>
                <span>{{ change.changedBy }}</span>
                <small>{{ formatDateTime(change.changedAt) }}</small>
              </li>
            </ul>
            <p v-else class="empty-state compact">No recent status history.</p>
          </div>
        </div>

        <div v-else class="empty-state large">Select a work order to view details.</div>
      </section>

      <aside class="panel create-panel">
        <div class="panel-header">
          <h2>Create work order</h2>
        </div>

        <div class="form-grid">
          <label>
            Title
            <input v-model="form.title" type="text" placeholder="New dispatch task" />
          </label>

          <label>
            Assigned to
            <input v-model="form.assignedTo" type="text" placeholder="Dispatcher name" />
          </label>

          <label>
            Status
            <select v-model="form.status">
              <option v-for="status in statuses" :key="status" :value="status">
                {{ status }}
              </option>
            </select>
          </label>

          <label>
            Priority
            <select v-model="form.priority">
              <option v-for="priority in priorities" :key="priority" :value="priority">
                {{ priority }}
              </option>
            </select>
          </label>

          <label>
            Due date
            <input v-model="form.dueDate" type="date" />
          </label>

          <button class="primary" @click="createWorkOrder">Create</button>
        </div>
      </aside>
    </main>
  </div>
</template>

<style scoped>
:global(body) {
  margin: 0;
  font-family: 'Segoe UI', sans-serif;
  background: #0e1724;
  color: #e5edf7;
}

* {
  box-sizing: border-box;
}

button,
input,
select {
  font: inherit;
}

.page-shell {
  max-width: 1500px;
  margin: 0 auto;
  padding: 32px 20px 48px;
}

.topbar {
  margin-bottom: 24px;
}

.eyebrow {
  margin: 0 0 8px;
  text-transform: uppercase;
  letter-spacing: 0.12em;
  font-size: 0.72rem;
  color: #8ec5ff;
}

h1 {
  margin: 0;
  font-size: 3rem;
}

h2,
h3 {
  margin: 0;
}

.layout {
  display: grid;
  grid-template-columns: 1.45fr 1.2fr 0.9fr;
  gap: 20px;
  align-items: start;
}

.panel {
  background: rgba(16, 26, 38, 0.9);
  border: 1px solid rgba(145, 172, 201, 0.4);
  border-radius: 16px;
  overflow: hidden;
  box-shadow: 0 8px 30px rgba(0, 0, 0, 0.18);
}

.panel-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 18px 18px 12px;
  border-bottom: 1px solid rgba(145, 172, 201, 0.2);
}

.toolbar select,
.form-grid input,
.form-grid select,
.inline-form input,
.inline-form select {
  width: 100%;
  background: rgba(10, 17, 25, 0.7);
  border: 1px solid rgba(145, 172, 201, 0.35);
  color: #edf5ff;
  border-radius: 10px;
  padding: 10px 12px;
}

.work-order-list {
  list-style: none;
  margin: 0;
  padding: 12px;
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.work-order-item {
  border: 1px solid rgba(145, 172, 201, 0.25);
  background: rgba(19, 32, 47, 0.9);
  border-radius: 12px;
  padding: 14px 14px 12px;
  cursor: pointer;
  transition: transform 0.15s ease, border-color 0.15s ease;
}

.work-order-item:hover,
.work-order-item.active {
  border-color: #6ab5ff;
  transform: translateY(-1px);
}

.meta-row,
.line-items,
.status-row,
.inline-form {
  display: flex;
  gap: 10px;
  align-items: center;
  flex-wrap: wrap;
}

.line-items {
  justify-content: space-between;
  margin-top: 10px;
  font-size: 0.82rem;
  color: #afc3d9;
}

.status-pill,
.priority-pill {
  display: inline-flex;
  align-items: center;
  border-radius: 999px;
  padding: 5px 10px;
  font-size: 0.72rem;
  font-weight: 700;
  letter-spacing: 0.04em;
  text-transform: uppercase;
}

.status-pill.Open,
.priority-pill.Low { background: rgba(59, 130, 246, 0.2); color: #a6d0ff; }
.status-pill.InProgress,
.priority-pill.Medium { background: rgba(245, 158, 11, 0.18); color: #f8d986; }
.status-pill.Completed,
.priority-pill.High { background: rgba(34, 197, 94, 0.18); color: #9fe7b0; }
.status-pill.OnHold,
.priority-pill.Urgent { background: rgba(239, 68, 68, 0.18); color: #f9a3a3; }

.detail-card,
.form-grid,
.history-box,
.status-update-box {
  padding: 18px;
}

.detail-grid {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 16px;
  margin: 18px 0;
}

.detail-grid label,
.form-grid label {
  display: block;
  color: #b9cae0;
  font-size: 0.78rem;
  margin-bottom: 6px;
}

.detail-grid div {
  background: rgba(12, 20, 28, 0.7);
  border: 1px solid rgba(145, 172, 201, 0.25);
  border-radius: 10px;
  padding: 10px 12px;
}

.inline-form {
  margin-top: 12px;
  display: grid;
  grid-template-columns: 180px 1fr auto;
}

.form-grid {
  display: grid;
  gap: 14px;
}

button.primary,
.inline-form button {
  border: none;
  border-radius: 10px;
  padding: 11px 16px;
  cursor: pointer;
  background: linear-gradient(135deg, #3ea0ff, #5ec4ff);
  color: #071722;
  font-weight: 700;
}

.history-list {
  list-style: none;
  margin: 12px 0 0;
  padding: 0;
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.history-list li {
  background: rgba(11, 18, 26, 0.7);
  border: 1px solid rgba(145, 172, 201, 0.2);
  border-radius: 10px;
  padding: 10px 12px;
  display: grid;
  gap: 5px;
}

.history-list small,
.empty-state {
  color: #afc3d9;
}

.banner {
  margin: 0 0 18px;
  padding: 12px 18px;
  border-radius: 10px;
  border: 1px solid rgba(239, 68, 68, 0.4);
  color: #ffcfcc;
  background: rgba(127, 29, 29, 0.25);
}

.empty-state {
  padding: 24px 18px;
  text-align: center;
}

.empty-state.large {
  min-height: 220px;
  display: grid;
  place-items: center;
}

@media (max-width: 1100px) {
  .layout {
    grid-template-columns: 1fr;
  }
}
</style>

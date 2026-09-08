const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5085'

async function request(path, options = {}) {
  const defaultHeaders = {
    'Content-Type': 'application/json',
  }

  const response = await fetch(`${API_BASE_URL}${path}`, {
    ...options,
    headers: {
      ...defaultHeaders,
      ...(options.headers || {}),
    },
  })

  if (!response.ok) {
    const text = await response.text()
    throw new Error(text || 'Request failed')
  }

  if (response.status === 204) {
    return null
  }

  return response.json()
}

export const api = {
  getWorkOrders(status = '') {
    const qs = status ? `?status=${encodeURIComponent(status)}` : ''
    return request(`/api/WorkOrders${qs}`)
  },

  getWorkOrder(id) {
    return request(`/api/WorkOrders/${id}`)
  },

  createWorkOrder(payload) {
    return request('/api/WorkOrders', {
      method: 'POST',
      body: JSON.stringify(payload),
    })
  },

  updateWorkOrder(id, payload) {
    return request(`/api/WorkOrders/${id}`, {
      method: 'PUT',
      body: JSON.stringify(payload),
    })
  },

  updateStatus(id, payload) {
    return request(`/api/WorkOrders/${id}/status`, {
      method: 'PATCH',
      body: JSON.stringify(payload),
    })
  },
}

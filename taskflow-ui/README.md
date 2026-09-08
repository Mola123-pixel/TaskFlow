# TaskFlow UI

TaskFlow UI is the frontend dashboard for a work-order and dispatch management application. It provides a fast, browser-based interface for viewing work orders, filtering by status, creating new tasks, and updating lifecycle status with a detailed history trail.

This project is built with Vue 3 and Vite, and it communicates with a backend API exposed through the `VITE_API_BASE_URL` environment variable.

## Overview

The interface is designed for operations teams managing work orders such as dispatch tasks, maintenance requests, and assignment tracking. Users can:

- View all work orders in a queue
- Filter by status
- Select a work order to inspect its details
- Create a new work order
- Update the status of an existing work order
- Review recent status change history

## Features

- Responsive dashboard layout
- Filterable work-order list by status
- Detailed selection panel for each work order
- Status update workflow with assigned user and change metadata
- Priority and due-date tracking
- Form validation for required work-order fields
- History timeline for status transitions
- Built on Vue 3 Composition API

## Tech Stack

- Vue 3
- Vite
- JavaScript
- Fetch API for backend communication

## Project Structure

```text
taskflow-ui/
├── public/
├── src/
│   ├── api.js
│   ├── App.vue
│   ├── main.js
│   └── assets/
├── index.html
├── package.json
├── vite.config.js
└── README.md
```

## Prerequisites

Before running this application, make sure you have:

- Node.js 18 or newer
- npm or another compatible package manager
- A running TaskFlow backend API

## Backend Dependency

This UI expects a backend API to be available at:

- Default local URL: `http://localhost:5085`
- Override via environment variable: `VITE_API_BASE_URL`

The frontend calls endpoints such as:

- `GET /api/WorkOrders`
- `GET /api/WorkOrders/{id}`
- `POST /api/WorkOrders`
- `PATCH /api/WorkOrders/{id}/status`

## Installation

1. Clone the repository.
2. Change into the project directory:

```bash
cd taskflow-ui
```

3. Install dependencies:

```bash
npm install
```

## Configuration

Create a `.env` file in the project root if you need to override the default API endpoint:

```env
VITE_API_BASE_URL=http://localhost:5085
```

If you do not set this variable, the app will default to `http://localhost:5085`.

## Available Scripts

Run the app in development mode:

```bash
npm run dev
```

Build for production:

```bash
npm run build
```

Preview the production build locally:

```bash
npm run preview
```

## Running the App

After installing dependencies, start the app with:

```bash
npm run dev
```

Then open the local Vite URL shown in the terminal, typically:

```text
http://localhost:5173
```

## Application Workflow

### Viewing work orders

- The app loads all work orders on startup
- The status filter dropdown narrows the queue
- Clicking a work order loads its details in the center panel

### Creating a work order

Use the create form in the right-hand panel to add a new item. Required fields include:

- Title
- Assigned to
- Due date

### Updating status

When a work order is selected:

- choose a new status from the update panel
- optionally provide the name of the person making the change
- save the update to send a PATCH request to the backend

## Data Model

The forms and UI are built around a work-order model similar to:

```json
{
  "id": 1,
  "title": "Inspect delivery route",
  "status": "Open",
  "priority": "Medium",
  "assignedTo": "Dispatcher A",
  "dueDate": "2026-09-10T00:00:00Z",
  "statusChanges": [
    {
      "id": 1,
      "fromStatus": "Open",
      "toStatus": "InProgress",
      "changedBy": "Dispatcher",
      "changedAt": "2026-09-08T10:00:00Z"
    }
  ]
}
```

## Status Values

Supported values in the current UI include:

- `Open`
- `InProgress`
- `Completed`
- `OnHold`

Priority values include:

- `Low`
- `Medium`
- `High`
- `Urgent`

## Troubleshooting

### API connection errors

If the app shows an error while loading data:

- confirm the backend service is running
- verify the API URL in `.env`
- check that the backend is listening on the expected port

### Blank or stale data

- refresh the browser after starting the backend
- confirm the browser console for fetch errors
- ensure the backend returns valid JSON for the work-order endpoints

## Notes

This frontend is intentionally focused on the task management workflow and is designed to pair with a TaskFlow backend service that exposes the work-order endpoints described above.

## License

This project is currently unlicensed unless otherwise specified by the repository owner.

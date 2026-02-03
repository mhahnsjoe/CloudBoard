import { ref } from 'vue'

export type ToastType = 'success' | 'error' | 'info' | 'warning'

export interface Toast {
    id: number
    type: ToastType
    message: string
    duration?: number
}

const toasts = ref<Toast[]>([])
let nextId = 1

export function useToast() {
    const addToast = (type: ToastType, message: string, duration = 3000) => {
        const id = nextId++
        const toast: Toast = { id, type, message, duration }
        toasts.value.push(toast)

        if (duration > 0) {
            setTimeout(() => {
                removeToast(id)
            }, duration)
        }

        return id
    }

    const removeToast = (id: number) => {
        const index = toasts.value.findIndex(t => t.id === id)
        if (index !== -1) {
            toasts.value.splice(index, 1)
        }
    }

    return {
        toasts,
        success: (message: string, duration?: number) => addToast('success', message, duration),
        error: (message: string, duration?: number) => addToast('error', message, duration),
        info: (message: string, duration?: number) => addToast('info', message, duration),
        warning: (message: string, duration?: number) => addToast('warning', message, duration),
        removeToast
    }
}

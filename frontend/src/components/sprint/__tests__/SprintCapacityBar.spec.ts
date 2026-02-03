import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import SprintCapacityBar from '../SprintCapacityBar.vue'

describe('SprintCapacityBar', () => {
    it('renders capacity information correctly', () => {
        const wrapper = mount(SprintCapacityBar, {
            props: { used: 40, total: 80 }
        })

        expect(wrapper.text()).toContain('40h / 80h')
        expect(wrapper.text()).toContain('50%')
    })

    it('shows green bar when under 70% capacity', () => {
        const wrapper = mount(SprintCapacityBar, {
            props: { used: 40, total: 80 }
        })

        const bar = wrapper.find('.bg-green-500')
        expect(bar.exists()).toBe(true)
    })

    it('shows yellow bar when between 90-100% capacity', () => {
        const wrapper = mount(SprintCapacityBar, {
            props: { used: 76, total: 80 }
        })

        const bar = wrapper.find('.bg-yellow-500')
        expect(bar.exists()).toBe(true)
    })

    it('shows red bar and warning when over capacity', () => {
        const wrapper = mount(SprintCapacityBar, {
            props: { used: 90, total: 80 }
        })

        const bar = wrapper.find('.bg-red-500')
        expect(bar.exists()).toBe(true)
        expect(wrapper.text()).toContain('Over capacity')
    })

    it('handles zero total capacity gracefully', () => {
        const wrapper = mount(SprintCapacityBar, {
            props: { used: 0, total: 0 }
        })

        expect(wrapper.text()).toContain('0%')
    })
})

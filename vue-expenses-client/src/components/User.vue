<template>
    <v-data-table
        :headers="headers"
        :items="users"
        sort-by="name"
        :items-per-page="5"
        loading-text="Loading... Please wait"
    >
        <template v-slot:top>
            <div class="d-flex align-center pa-1 pb-2">
                <span class="blue--text font-weight-medium">Users</span>
                <v-divider
                    class="mx-2 my-1"
                    inset
                    vertical
                    style="height: 20px"
                ></v-divider>
                <v-spacer></v-spacer>
            </div>
        </template>
        <template v-slot:item.colour="{ item }">
            <v-chip
                :color="item.colour"
                style="padding: 0px; height: 20px; width: 20px"
                flat
                small
                class="ml-1 mb-1"
            ></v-chip>
        </template>
        <template v-slot:no-data>
            <span>No Data Available</span>
        </template>
    </v-data-table>
</template>

<script lang="ts">
import { defineComponent, computed } from '@vue/composition-api'
import store from '../store'

export default defineComponent({
    setup({}) {
        const loading = false
        const dialog = false
        const headers = [
            { text: 'Id', value: 'id', align: ' d-none' },
            { text: 'Name', value: 'email' },
            { text: 'Description', value: 'description' },
            { text: 'Actions', value: 'action', sortable: false, width: 50 }
        ]
        const users = computed(() => {
            return store.state.users.users
        })
        console.log('hallo users', users)
        return {
            loading,
            dialog,
            headers,
            users
        }
    }
})
</script>

<style></style>

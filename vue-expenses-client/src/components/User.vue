<template>
    <v-data-table
        :headers="headers"
        :items="users"
        sort-by="email"
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
                <v-dialog v-model="dialog" max-width="500px">
                    <template v-slot:activator="{ on }">
                        <v-btn
                            outlined
                            small
                            class="blue--text font-weight-bold"
                            v-on="on"
                            >New User</v-btn
                        >
                    </template>
                    <v-card>
                        <v-card-title>
                            <span class="text-h5">New User</span>
                        </v-card-title>

                        <v-card-text>
                            <v-container>
                                <input type="hidden" v-model="editedUser.id" />
                                <v-text-field
                                    class="ma-0 pa-0 form-label"
                                    dense
                                    v-model="editedUser.email"
                                    :rules="[required('Email'), email()]"
                                    label="E-Mail"
                                ></v-text-field>
                                <v-text-field
                                    v-model="editedUser.firstName"
                                    placeholder="First Name"
                                    :rules="[required('FirstName')]"
                                    dense
                                ></v-text-field>
                                <v-text-field
                                    v-model="editedUser.lastName"
                                    placeholder="Last Name"
                                    dense
                                ></v-text-field>
                                <v-text-field
                                    v-model="editedUser.password"
                                    placeholder="Password"
                                    :type="
                                        showRegisterPassword
                                            ? 'text'
                                            : 'password'
                                    "
                                    :append-icon="
                                        showRegisterPassword
                                            ? 'mdi-eye'
                                            : 'mdi-eye-off'
                                    "
                                    :rules="[required('Password')]"
                                    dense
                                >
                                    <v-icon
                                        slot="append"
                                        small
                                        v-if="showRegisterPassword"
                                        @click="
                                            showRegisterPassword =
                                                !showRegisterPassword
                                        "
                                        >mdi-eye</v-icon
                                    >
                                    <v-icon
                                        slot="append"
                                        small
                                        v-else
                                        @click="
                                            showRegisterPassword =
                                                !showRegisterPassword
                                        "
                                        >mdi-eye-off</v-icon
                                    >
                                </v-text-field>
                            </v-container>
                        </v-card-text>

                        <v-card-actions>
                            <v-spacer></v-spacer>
                            <v-btn
                                outlined
                                small
                                class="blue--text font-weight-bold"
                                @click="saveUser"
                                :loading="loading"
                                >Save</v-btn
                            >
                            <v-btn
                                outlined
                                small
                                class="blue--text font-weight-bold"
                                @click="close"
                                >Cancel</v-btn
                            >
                        </v-card-actions>
                    </v-card>
                </v-dialog>
            </div>
        </template>
        <template v-slot:no-data>
            <span>No Data Available</span>
        </template>
    </v-data-table>
</template>

<script lang="ts">
import { defineComponent, computed, ref } from '@vue/composition-api'
import store from '../store'

export default defineComponent({
    setup({}) {
        const loading = false
        const dialog = ref(false)
        let editedUser = {
            id: 0,
            email: '',
            firstName: '',
            lastName: '',
            password: ''
        }
        const defaultUser = {
            id: 0,
            email: '',
            firstName: '',
            lastName: '',
            password: ''
        }
        const headers = [
            { text: 'Id', value: 'id', align: ' d-none' },
            { text: 'E-Mail', value: 'email' },
            { text: 'Name', value: 'fullName' },
            { text: 'Actions', value: 'action', sortable: false, width: 50 }
        ]
        const users = computed(() => {
            return store.state.users.users
        })
        const close = () => {
            dialog.value = false
            editedUser = Object.assign({}, defaultUser)
        }
        const saveUser = () => {
            const editedUserCopy = editedUser
        }
        const showRegisterPassword = false
        console.log('hallo users', users)
        return {
            close,
            saveUser,
            showRegisterPassword,
            loading,
            editedUser,
            dialog,
            headers,
            users
        }
    }
})
</script>

<style></style>

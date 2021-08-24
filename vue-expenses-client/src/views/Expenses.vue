<template>
    <div>
        <v-container>
            <v-layout row>
                <v-flex xs12>
                    <v-data-table
                        :headers="headers"
                        :items="expenses"
                        :options.sync="pagination"
                        footer-props.items-per-page-options="pagination.rowsPerPageItems"
                        :server-items-length="pagination.totalItems"
                        :loading="loading"
                        class="elevation-1"
                    >
                        <template v-slot:top>
                            <div class="d-flex align-center pa-4">
                                <span class="blue--text font-weight-medium"
                                    >Expenses</span
                                >
                                <v-divider
                                    class="mx-2 my-1"
                                    inset
                                    vertical
                                    style="height: 20px"
                                ></v-divider>
                                <v-spacer></v-spacer>
                                <v-dialog v-model="dialog" max-width="650px">
                                    <template v-slot:activator="{ on }">
                                        <v-btn
                                            outlined
                                            small
                                            class="blue--text font-weight-bold"
                                            v-on="on"
                                            >New Expense</v-btn
                                        >
                                    </template>
                                    <v-card>
                                        <v-card-title>
                                            <span class="text-h5">{{
                                                formTitle
                                            }}</span>
                                        </v-card-title>

                                        <v-card-text>
                                            <ExpenseForm
                                                :expense="editedExpense"
                                                :showCloseButton="true"
                                                :onCloseClick="close"
                                                :onSubmitClick="saveExpense"
                                                :loading="loading"
                                                ref="form"
                                            />
                                        </v-card-text>
                                    </v-card>
                                </v-dialog>
                            </div>
                        </template>
                        <template v-slot:item.value="{ item }">
                            <span>{{
                                `${user.displayCurrency} ${item.value.toFixed(
                                    2
                                )}`
                            }}</span>
                        </template>
                        <template v-slot:item.action="{ item }">
                            <v-icon
                                small
                                class="mr-2"
                                @click="editExpense(item)"
                                >edit</v-icon
                            >
                            <v-icon small @click="deleteExpense(item)"
                                >delete</v-icon
                            >
                        </template>
                    </v-data-table>
                </v-flex>
            </v-layout>
        </v-container>
    </div>
</template>
<script lang="ts">
import { mapState, mapActions } from 'vuex'
import {
    CREATE_EXPENSE,
    EDIT_EXPENSE,
    REMOVE_EXPENSE,
    LOAD_EXPENSES
} from '@/store/_actiontypes'
import { SET_EXPENSES_PAGINATION } from '@/store/_mutationtypes'
import store from '../store'
import ExpenseForm from '../components/ExpenseForm.vue'
import { defineComponent } from '@vue/composition-api'

export default defineComponent({
    components: { ExpenseForm },
    data: () => ({
        loading: false,
        dialog: false,
        headers: [
            { text: 'Id', value: 'id', align: ' d-none' },
            { text: 'CategoryId', value: 'categoryId', align: ' d-none' },
            { text: 'TypeId', value: 'typeId', align: ' d-none' },
            { text: 'Value', value: 'value' },
            { text: 'Date', value: 'date' },
            { text: 'Category', value: 'category' },
            { text: 'Type', value: 'type' },
            { text: 'Description', value: 'comments' },
            { text: 'Actions', value: 'action', sortable: false, width: 50 }
        ],
        editedExpense: {
            id: 0,
            date: '',
            value: '',
            categoryId: '',
            typeId: '',
            comments: ''
        },
        defaultExpense: {
            id: 0,
            date: '',
            value: '',
            categoryId: '',
            typeId: '',
            comments: ''
        }
    }),
    computed: {
        ...mapState({
            expenses: (state: any) => state.expenses.expenses,
            user: (state: any) => state.account.user
        }),

        formTitle() {
            return this.editedExpense.id === 0 ? 'New Expense' : 'Edit Expense'
        },
        pagination: {
            get: function () {
                return store.state.expenses.pagination
            },
            set: function (value) {
                console.log('hello', value)
                store.commit(`expenses/${SET_EXPENSES_PAGINATION}`, value)
            }
        }
    },
    watch: {
        dialog(val) {
            val || this.close()
        },
        pagination: {
            async handler() {
                this.loading = true
                await this.$store.dispatch(`expenses/${LOAD_EXPENSES}`)
                this.loading = false
            },
            deep: true
        }
    },
    methods: {
        ...mapActions('expenses', [
            CREATE_EXPENSE,
            EDIT_EXPENSE,
            REMOVE_EXPENSE
        ]),

        editExpense(item) {
            this.editedExpense = Object.assign({}, item)
            this.dialog = true
        },

        deleteExpense(item) {
            confirm('Are you sure you want to delete this item?') &&
                this.REMOVE_EXPENSE({ id: item.id })
        },

        close() {
            this.dialog = false
            this.editedExpense = Object.assign({}, this.defaultExpense)
            this.$refs.form.reset()
        },

        saveExpense() {
            var editedExpense = this.editedExpense
            this.loading = true
            if (editedExpense.id == 0) {
                this.CREATE_EXPENSE({
                    expense: editedExpense
                })
                    .then(() => {
                        this.close()
                    })
                    .finally(() => {
                        this.loading = false
                    })
            } else {
                this.EDIT_EXPENSE({
                    expense: editedExpense
                })
                    .then(() => {
                        this.close()
                    })
                    .finally(() => {
                        this.loading = false
                    })
            }
        }
    }
})
</script>

<style></style>

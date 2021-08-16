<template>
    <v-form class="xs12 my-1">
        <v-container>
            <v-file-input
                show-size
                multiple
                label="CSV Import File"
                @change="selectFile"
            ></v-file-input>
            <v-row class="justify-end">
                <v-btn
                    outlined
                    small
                    class="blue--text font-weight-bold"
                    @click="handleSubmit"
                    >Import</v-btn
                >
            </v-row>
        </v-container>
    </v-form>
</template>

<script lang="ts">
import { defineComponent } from '@vue/composition-api'
import Api from '../services/api'

export default defineComponent({
    setup(props) {
        let currentFile = null
        console.log(props)
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        const selectFile = (file: any) => {
            if (file) {
                console.log(file)
                currentFile = file
            }
        }
        const handleSubmit = async () => {
            if (currentFile != null) {
                console.log(currentFile)
                let formData = new FormData()
                formData.append('file', currentFile[0], currentFile[0].name)
                await Api.post('/expenses/import', formData, {
                    headers: {
                        'Content-Type': 'multipart/form-data'
                    }
                })
            }
        }
        return {
            handleSubmit,
            selectFile
        }
    }
})
</script>

<style></style>

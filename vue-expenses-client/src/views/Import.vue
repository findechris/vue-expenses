<template>
    <div>
        <v-container>
            <v-layout row justify-space-between>
                <v-flex xs12>
                    <v-layout row justify-space-between>
                        <v-flex xs12 md4 my-3>
                            <v-card
                                class="pa-2 mr-2"
                                flat
                                min-height="340px"
                                height="100%"
                            >
                                <v-file-input
                                    show-size
                                    multiple
                                    label="File input"
                                    @change="selectFile"
                                ></v-file-input>
                            </v-card>
                        </v-flex>
                    </v-layout>
                </v-flex>
            </v-layout>
        </v-container>
    </div>
</template>

<script lang="ts">
import { defineComponent } from '@vue/composition-api'
import Api from '../services/api'

export default defineComponent({
    setup(props) {
        // let currentFile = null;
        console.log(props)
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        const selectFile = async (file: any) => {
            if (file) {
                console.log(file)
                let formData = new FormData()
                formData.append('file', file[0], file[0].name)
                await Api.post('/expenses/import', formData, {
                    headers: {
                        'Content-Type': 'multipart/form-data'
                    }
                })
            }
        }
        return {
            selectFile
        }
    }
})
</script>

<style></style>

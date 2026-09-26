class MultiSelect {
    static create(options, existingValues, hiddenInput, wrapper, title) {
        let existingSelects = []
        wrapper.className = 'multi-select-section'

        const updateHiddenInput = () => {
            const newValue = existingSelects.map(e => e.value).filter(e => e).join(',')
            hiddenInput.value = newValue
        }

        const createExistingSelect = (selectedId) => {
            const inputWrapper = HtmlUtils.addElement('div', 'input-wrapper', existingWrapper)
            const select = HtmlUtils.addElement('select', 'multi-select', inputWrapper)
            select.onchange = updateHiddenInput
            HtmlUtils.fillDropdown(select, options, selectedId)
            existingSelects.push(select)

            const deleteButton = HtmlUtils.addElement('button', 'recipe-button', inputWrapper)
            deleteButton.type = 'button'
            deleteButton.innerText = 'X'
            deleteButton.onclick = () => {
                existingSelects = existingSelects.filter(x => x !== select)
                inputWrapper.remove()
                updateHiddenInput()
            }
        }

        const titleElement = HtmlUtils.addElement('span', '', wrapper)
		titleElement.innerText = title

        const existingWrapper = HtmlUtils.addElement('div', 'existing-multi-select', wrapper)
        for (const existingValue of existingValues) {
            createExistingSelect(existingValue)
        }

        const newInputWrapper = HtmlUtils.addElement('div', 'input-wrapper', wrapper)
        let newSelect = HtmlUtils.addElement('select', 'multi-select', newInputWrapper)
        HtmlUtils.addElement('option', '', newSelect)

        HtmlUtils.fillDropdown(newSelect, options)
        newSelect.onchange = () => {
            createExistingSelect(newSelect.value)
            newSelect.value = ''
            updateHiddenInput()
        }

        updateHiddenInput()
    }
}

class StarRanker {
	static create(callback, maxRanking, hideNote) {
		const outer = HtmlUtils.createElement('div', 'star-wrapper-outer');
		const wrapper = HtmlUtils.addElement('div', 'star-wrapper', outer);
		maxRanking = maxRanking || 10
		for (let i = 1; i <= maxRanking; i++) {
			const star = HtmlUtils.addElement('span', 'star', wrapper)
			$(star).click(() => callback(i, $('#starRankNote').val()))
			star.innerText = i.toString();
		}

		const textField = HtmlUtils.addElement('textarea', '', outer)
		textField.id = 'starRankNote'
		if (hideNote) {
			textField.style.display = 'none'
		}

		return outer
	}
}
